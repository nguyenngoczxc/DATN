using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTTN3.Models;

namespace TTTN3.Controllers
{
    [AllowAnonymous]
    public class ContactController : Controller
    {
        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ContactController> _logger;

        public ContactController(ILogger<ContactController> logger, IWebHostEnvironment webHostEnvironment, DataContext _db)
        {
            _logger = logger;
            db = _db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
