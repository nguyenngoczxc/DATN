using MailKit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;
using System.Security.Policy;
using TTTN3.Interfaces;
using TTTN3.Models;
using TTTN3.Models.ViewModels;

namespace TTTN3.Controllers
{
    public class AccountController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signinManager;
        private readonly IMailSender _emailService;
        
        public AccountController(UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signinManager, IWebHostEnvironment webHostEnvironment, IMailSender emailService)
        {
            _userManager = userManager;
            _signinManager = signinManager;
            _webHostEnvironment = webHostEnvironment;
            _emailService = emailService;
        }
        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Microsoft.AspNetCore.Identity.SignInResult result = await _signinManager.PasswordSignInAsync(loginVM.user_Name, loginVM.password, false, false);
                    if (result.Succeeded)
                    {
                        var user = await _userManager.FindByNameAsync(loginVM.user_Name);
                        if (user != null)
                        {
                            var claims = new List<Claim>
    {
        new Claim("avatar", user.avatar) // Assuming user.Avatar is the URL or path to the avatar
    };
                            await _userManager.AddClaimsAsync(user, claims);

                            var roles = await _userManager.GetRolesAsync(user);
                            if (roles.Contains("Admin"))
                            {
                                return Redirect("/admin/homeadmin/Index");
                            }
                            else
                            {
                                return Redirect("/Home/Index");
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Usernam or password is wrong");

                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }
            return View(loginVM);
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Register(UserModel user, IFormFile anhDaiDienFile)
        public async Task<IActionResult> Register(UserModel user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    user.avatar = "avatar.jpg";
                    AppUserModel newUser = new AppUserModel { UserName = user.user_Name, Email = user.email, avatar = user.avatar };
                    IdentityResult result = await _userManager.CreateAsync(newUser, user.password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(newUser, "customer");

                        var emailSubject = "Successful account registration";
                        var emailBody = "You have successfully registered for Minh Ngoc Shop's customer account";
                        await _emailService.SendEmailAsync(newUser.Email, emailSubject, emailBody);

                        return Redirect("/account/Login");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }
            return View(user);
        }
        public async Task<IActionResult> Logout()
        {
            await _signinManager.SignOutAsync();
            return Redirect("/account/login");
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                user_Name = user.UserName,
                email = user.Email,
                phone = user.phone,
                address = user.address,
                avatar = user.avatar // Gán đường dẫn avatar của người dùng vào model
            };

            return View(model);
        }

        //POST: /Account/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model, IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
            {
                // Người dùng không chọn file, không cần xử lý, có thể gán giá trị mặc định cho anhDaiDienFile
                avatar = new FormFile(Stream.Null, 0, 0, "", "");
            }

            // Kiểm tra trạng thái của ModelState trước khi kiểm tra IsValid
            if (ModelState.IsValid || avatar != null)
            {
                var user = await _userManager.GetUserAsync(User); // Lấy thông tin người dùng hiện tại từ HttpContext.User
                if (user == null)
                {
                    return NotFound(); // Trả về 404 nếu không tìm thấy người dùng
                }

                // Update user properties
                user.UserName = model.user_Name;
                user.Email = model.email;
                user.phone = model.phone;
                user.address = model.address;
                if (avatar != null && avatar.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(avatar.FileName);
                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/User");
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        avatar.CopyTo(stream);
                    }
                    user.avatar = uniqueFileName;
                }
                else
                {
                    user.avatar = user.avatar;
                }

                // Update password if provided
                if (!string.IsNullOrEmpty(model.password))
                {
                    var passwordChangeResult = await _userManager.ChangePasswordAsync(user, model.password_Old, model.password);
                    if (!passwordChangeResult.Succeeded)
                    {
                        foreach (var error in passwordChangeResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model); // Trả về lại view với model nếu có lỗi khi thay đổi mật khẩu
                    }
                }


                // Update user in database
                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    return Redirect("/account/Login"); // Redirect to home page after successful update
                }
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }

            // If ModelState is not valid or other errors occur, return to the edit view with the provided model
            return View(model);
        }


        // GET: /Account/Delete
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /Account/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(user);
        }
        // AccountController.cs

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            var model = new ForgotPasswordViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // Generate a password reset token
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    // Generate the password reset link
                    var resetLink = Url.Action("ResetPassword", "Account", new { userId = user.Id, token }, protocol: HttpContext.Request.Scheme);

                    // Compose the email
                    var emailSubject = "Password Reset Request";
                    var emailBody = $"Please click the following <a href='{resetLink}'>link</a> to reset your password.";

                    // Send the email
                    await _emailService.SendEmailAsync(model.Email, emailSubject, emailBody);

                    // Set a flag to indicate that the email has been sent
                    model.EmailSent = true;
                }
                else
                {
                    // If the user is not found, still set EmailSent to true to avoid leaking information
                    model.EmailSent = true;
                }
            }
            return View(model);
        }

        // AccountController.cs

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string userId, string token)
        {
            // Retrieve the user based on the userId
            var user = await _userManager.FindByIdAsync(userId);

            // Check if the user exists
            if (user == null)
            {
                // Handle the case where the user is not found
                return RedirectToAction("Error");
            }

            // Check if the token is valid for the user
            var isValidToken = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, UserManager<AppUserModel>.ResetPasswordTokenPurpose, token);

            if (!isValidToken)
            {
                // Handle the case where the token is not valid
                return RedirectToAction("Error");
            }

            // Pass the username to the view
            ViewBag.Username = user.UserName;

            var model = new ResetPasswordViewModel { UserId = userId, Token = token };
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        // Password reset successful, redirect to login page
                        return RedirectToAction("Login");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "User not found");
                }
            }
            return View(model);
        }

        // AccountController.cs


        //[AllowAnonymous]
        //public IActionResult ExternalLoginFacebook()
        //{
        //    var redirectUrl = Url.Action("ExternalLoginCallback", "Account");
        //    var properties = _signinManager.ConfigureExternalAuthenticationProperties("Facebook", redirectUrl);
        //    return new ChallengeResult("Facebook", properties);
        //}
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginFacebook()
        {
            // Clear any existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var redirectUrl = Url.Action("ExternalLoginCallback", "Account");
            var properties = _signinManager.ConfigureExternalAuthenticationProperties("Facebook", redirectUrl);
            return new ChallengeResult("Facebook", properties);
        }


        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            // Retrieve the external login information
            var info = await _signinManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["ErrorMessage"] = "External login information not available.";
                // Handle the case where external login information is not available
                return RedirectToAction("Login");
            }

            // Check if the user is already signed in
            var userl = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (userl != null)
            {
                // User is already registered, sign in and redirect
                await _signinManager.SignInAsync(userl, isPersistent: false) ;
                return Redirect("/home/index");
            }
            else
            {
                // User is not registered, proceed with registration

                // Extract user information from external login provider
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var nameClaim = info.Principal.FindFirst("preferred_username")
                    ?? info.Principal.FindFirst(ClaimTypes.Name)
                    ?? info.Principal.FindFirst(ClaimTypes.GivenName);
                var password = Guid.NewGuid().ToString("N").Substring(0, 8);
                var pictureClaim = info.Principal.FindFirst("picture") ?? info.Principal.FindFirst("picture-url");
                string avatarUrl = null;
                if (pictureClaim != null)
                {
                    avatarUrl = pictureClaim.Value;
                }
                else
                {
                    // Nếu không tìm thấy claim nào chứa đường dẫn avatar, có thể set một URL mặc định hoặc xử lý theo ý muốn của bạn.
                    avatarUrl = "avatar.jpg";
                }

                string userName = null;
                if (nameClaim != null)
                {
                    // Check if the username already exists in the database
                    var existingUser = await _userManager.FindByNameAsync(nameClaim.Value);
                    if (existingUser != null)
                    {
                        // If the username exists, set it to the email
                        userName = email;
                    }
                    else
                    {
                        userName = nameClaim.Value;
                    }
                }
                else
                {
                    // Nếu không tìm thấy claim nào chứa tên, có thể xử lý theo ý muốn của bạn.
                    // Chẳng hạn, bạn có thể đặt userName bằng giá trị mặc định hoặc sử dụng email làm tên.
                    userName = email;
                }


                // Create a new user
                var user = new AppUserModel { UserName = userName, Email = email,avatar= avatarUrl };

                // Attempt to create the user
                var createResult = await _userManager.CreateAsync(user,password);
                if (createResult.Succeeded)
                {
                    await _userManager.AddLoginAsync(user, info);
                    // Add the user to the "customer" role
                    await _userManager.AddToRoleAsync(user, "customer");

                    // Sign in the user
                    await _signinManager.SignInAsync(user, isPersistent: false);

                    // Redirect to the appropriate page
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Handle the case where user creation fails
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError("", "Error: " + error.Description);
                        TempData["ErrorMessage"] = error.Description;
                    }
                    return RedirectToAction("Login");
                }
            }
        }


    }
}

