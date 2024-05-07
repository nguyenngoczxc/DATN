using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTTN3.Models;
using X.PagedList;

namespace TTTN3.Controllers
{
    [Route("Promotion")]
    [AllowAnonymous]
    public class PromotionController : Controller
    {
        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<PromotionController> _logger;

        public PromotionController(ILogger<PromotionController> logger, IWebHostEnvironment webHostEnvironment, DataContext _db)
        {
            _logger = logger;
            db = _db;
            _webHostEnvironment = webHostEnvironment;
        }
       
        [HttpGet]
        [Route("Promotion_Detail")]

        public IActionResult Promotion_Detail()
        {
            var promotion = db.promotion.SingleOrDefault(x => x.select == true);
            return View(promotion);
        }
    }
}
