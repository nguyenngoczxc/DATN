using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TTTN3.Models;
using TTTN3.Models.ViewModels;
using TTTN3.Responsitory;

namespace TTTN3.ViewComponents
{
    public class CheckOut_User : ViewComponent
    {
        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signinManager;
        public CheckOut_User(DataContext _db, UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signinManager, IWebHostEnvironment webHostEnvironment)
        {
            db = _db;
            _userManager = userManager;
            _signinManager = signinManager;
            _webHostEnvironment = webHostEnvironment;
        }
        public IViewComponentResult Invoke()
        {
            var model = new InvoiceViewModel();

            if (User.Identity.IsAuthenticated)
            {
                var user = _userManager.FindByNameAsync(User.Identity.Name).Result;

                if (user != null)
                {
                    ViewBag.User = user;
                    // Nếu người dùng đã đăng nhập, điền thông tin từ thông tin người dùng vào form
                    model.CustomerName = user.UserName;
                    model.Email = user.Email;
                    model.Address = user.address;
                    model.Phone = user.phone;
                    model.CustomerId = user.Id;
                }
            }
            else
            {
                // Nếu người dùng không đăng nhập, tạo một đối tượng trống và trả về
                ViewBag.User = null;
                model.CustomerName = string.Empty;
                model.Email = string.Empty;
                model.Address = string.Empty;
                model.Phone = string.Empty;
                model.CustomerId = null; // Hoặc giá trị phù hợp khác nếu CustomerId là một kiểu dữ liệu cụ thể
            }

            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var error = ModelState[key].Errors.FirstOrDefault();
                    if (error != null)
                    {
                        ModelState.AddModelError(key, error.ErrorMessage);
                    }
                }
                return View("CheckOut_User", model);
            }

            // Nếu không có lỗi, tiếp tục thực hiện logic của bạn

            return View("CheckOut_User", model);
        }

    }
}
