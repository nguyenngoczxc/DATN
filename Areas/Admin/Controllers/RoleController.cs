using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TTTN3.Models;

namespace TTTN3.Areas.Admin.Controllers
{

    // [Authorize(Roles = "Admin")]
     [Area("Admin")]
    [Route("admin")]
    [Authorize("Admin")]
    [Route("admin/role")]
    public class RoleController : Controller
    {
        // GET: Admin/Role
        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<RoleController> _logger; 
        private readonly RoleManager<IdentityRole> _roleManager;


        public RoleController(ILogger<RoleController> logger, IWebHostEnvironment webHostEnvironment, DataContext _db, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            db = _db;
            _webHostEnvironment = webHostEnvironment;
            _roleManager = roleManager;
        }
        [Route("Index_role")]
        public ActionResult Index_role()
        {
            var items = db.Roles.ToList();
            return View(items);
        }
        [Route("Create_role")]
        public ActionResult Create_role()
        {
            return View();
        }
        [HttpPost]
        [Route("Create_role")]
        [ValidateAntiForgeryToken]
        public ActionResult Create_role(IdentityRole model)
        {
            if (ModelState.IsValid)
            {
                // Khai báo các dependencies cần thiết
                var roleStore = new RoleStore<IdentityRole>(db);
                var roleValidators = new List<IRoleValidator<IdentityRole>>();
                var lookupNormalizer = new UpperInvariantLookupNormalizer();
                var identityErrorDescriber = new IdentityErrorDescriber();
                var logger = new Logger<RoleManager<IdentityRole>>(new LoggerFactory());

                // Khởi tạo RoleManager với các dependencies đã được khai báo
                var roleManager = new RoleManager<IdentityRole>(
                    roleStore,
                    roleValidators,
                    lookupNormalizer,
                    identityErrorDescriber,
                    logger
                );

                // Tiến hành tạo vai trò
                var result = roleManager.CreateAsync(model).Result;
                if (result.Succeeded)
                {
                    return RedirectToAction("Index_role");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }
        [Route("Edit_role")]
        public async Task<ActionResult> Edit_role(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }
        [Route("Edit_role")]
        [HttpPost]
        [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit_role(string id, string name)
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                role.Name = name;

                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index_role");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(role);
            }
        
        [HttpGet]
        [Route("delete_role")]
        public async Task<IActionResult> Delete_role(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // POST: /admin/role/delete/{id}
        [HttpPost, ActionName("Delete_role")]
        [ValidateAntiForgeryToken]
        [Route("delete_role")]
        public async Task<IActionResult> DeleteConfirmed_role(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return RedirectToAction("Index_role");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(role);
            }
        }
    
}
}