using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTTN3.Models;

namespace TTTN3.Controllers
{
    [AllowAnonymous]
    public class BrandController : Controller
    {
        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BrandController> _logger;

        public BrandController(ILogger<BrandController> logger, IWebHostEnvironment webHostEnvironment, DataContext _db)
        {
            _logger = logger;
            db = _db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult GetBrand()
        {
            var brands = db.brands.ToList();
            return Json(brands);
        }
    }
}
