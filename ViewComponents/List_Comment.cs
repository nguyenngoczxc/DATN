using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTTN3.Models;
using TTTN3.Models.ViewModels;
using TTTN3.Responsitory;

namespace TTTN3.ViewComponents
{
    public class List_Comment : ViewComponent
    {
        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signinManager;
        public List_Comment(DataContext _db, UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signinManager, IWebHostEnvironment webHostEnvironment)
        {
            db = _db;
            _userManager = userManager;
            _signinManager = signinManager;
            _webHostEnvironment = webHostEnvironment;
        }
        public IViewComponentResult Invoke(string product_Code)
        {
            var comments = db.comments
                .Include(c => c.AspNetUser)
                .Include(c => c.product)
                .Where(c => c.product_Code == product_Code)
                .ToList();

            return View("List_Comment", comments);
        }
    }
}
