using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TTTN3.Models;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using TTTN3.Models.ViewModels;

namespace TTTN3.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin")]
    [Route("admin/account")]
    [Authorize(Roles ="Admin")]
    public class AccountController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private UserManager<AppUserModel> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private SignInManager<AppUserModel> _signinManager;
        private readonly DataContext db;
        public AccountController(DataContext _db,RoleManager<IdentityRole> roleManager, UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signinManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signinManager = signinManager;
            _webHostEnvironment = webHostEnvironment;
            db = _db;
        }
        [Route("Index_account")]
        public IActionResult Index_account(string SearchText, int? page)
        {
            int pageSize = 5;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            IQueryable<AppUserModel> lstSanPham = _userManager.Users.AsNoTracking().OrderByDescending(x => x.UserName);
            if (!string.IsNullOrEmpty(SearchText))
            {
                lstSanPham = lstSanPham.Where(x => x.UserName.Contains(SearchText));
            }
            ViewData["SearchText"] = SearchText;

            //var lst = lstSanPham.ToPagedList(lstSanPham,pageNumber, pageSize);
            PagedList<AppUserModel> lst = new PagedList<AppUserModel>(lstSanPham, pageNumber, pageSize);
            return View(lst);
        }
        [Route("AddRole")]
        [HttpGet]
        public async Task<IActionResult> AddRole(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new AddRoleViewModel
            {
                RoleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync(),
                UserId = user.Id,
                UserName = user.UserName,
            };

            return View(model);
        }


        [Route("AddRole")]
        [HttpPost]
        public async Task<IActionResult> AddRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var selectedRole = roles.FirstOrDefault(r => r.Name == roleName);
            if (selectedRole != null)
            {
                var result = await _userManager.AddToRoleAsync(user, selectedRole.Name);
                if (result.Succeeded)
                {
                    // Thêm quyền thành công
                    return RedirectToAction("Index_account"); // Hoặc chuyển hướng đến trang khác
                }
                else
                {
                    // Xử lý lỗi khi thêm quyền
                    ModelState.AddModelError("", "Failed to add role to user.");
                }
            }
            else
            {
                // Xử lý khi quyền không tồn tại
                ModelState.AddModelError("", "Role does not exist.");
            }
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            // Trả về view với thông báo lỗi nếu có lỗi xảy ra
            return View(user);
        }

        [HttpGet]
        [Route("Delete_account")]
        public async Task<IActionResult> Delete_account(string id)
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
        [HttpPost, ActionName("Delete_account")]
        [Route("Delete_account")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed_account(string id)
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
            var favoriteProducts = db.FavoriteProduct.Where(fp => fp.UserId == id).ToList();

            if (favoriteProducts.Any())
            {
                // Remove all favorite products
                db.FavoriteProduct.RemoveRange(favoriteProducts);
                await db.SaveChangesAsync();
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index_account");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(user);
        }
    }
}
