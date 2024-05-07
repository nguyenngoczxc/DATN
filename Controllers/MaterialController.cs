using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTTN3.Models;

namespace TTTN3.Controllers
{
    [AllowAnonymous]
    public class MaterialController : Controller
    {
        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<MaterialController> _logger;

        public MaterialController(ILogger<MaterialController> logger, IWebHostEnvironment webHostEnvironment, DataContext _db)
        {
            _logger = logger;
            db = _db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult GetMaterial()
        {
            var materials = db.materials.ToList();
            return Json(materials);
        }
    }
}
