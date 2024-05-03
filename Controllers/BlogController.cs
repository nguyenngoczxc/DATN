using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTTN3.Models;
using X.PagedList;

namespace TTTN3.Controllers
{
        [Route("Blog")]
    [AllowAnonymous]
    public class BlogController : Controller
    {
        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BlogController> _logger;

        public BlogController(ILogger<BlogController> logger, IWebHostEnvironment webHostEnvironment, DataContext _db)
        {
            _logger = logger;
            db = _db;
            _webHostEnvironment = webHostEnvironment;
        }
        [Route("Index")]
        public IActionResult Index(string SearchText, int? page)
        {
            int pageSize = 6;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            IQueryable<blog> lstSanPham = db.blogs.AsNoTracking().OrderByDescending(x => x.blog_Date);
            if (!string.IsNullOrEmpty(SearchText))
            {
                lstSanPham = lstSanPham.Where(x => x.blog_Code.Equals(SearchText) || x.blog_Title.Contains(SearchText));
            }
            ViewData["SearchText"] = SearchText;

            //var lst = lstSanPham.ToPagedList(lstSanPham, pageNumber, pageSize);
            PagedList<blog> lst = new PagedList<blog>(lstSanPham, pageNumber, pageSize);
            return View(lst);
        }
        [HttpGet]
        public IActionResult Blog_Detail(string blog_Code)
        {
            var blog = db.blogs.SingleOrDefault(x => x.blog_Code == blog_Code);
            return View(blog);
        }
    }
}
