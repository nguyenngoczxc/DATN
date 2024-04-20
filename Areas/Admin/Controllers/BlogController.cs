using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using TTTN3.Models;
using X.PagedList;

namespace TTTN3.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin")]
    [Route("admin/Blog")]
    [Authorize("Admin")]

    public class BlogController : Controller
    {
        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BlogController(IWebHostEnvironment webHostEnvironment,DataContext _db) { 
            db = _db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Route("List_Blog")]
        
        public IActionResult List_Blog(string SearchText, int? page)
        {
            int pageSize = 5;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            IQueryable<blog> lstSanPham = db.blogs.AsNoTracking().OrderByDescending(x => x.blog_Date);
            if (!string.IsNullOrEmpty(SearchText))
            {
                lstSanPham = lstSanPham.Where(x => x.blog_Code.Equals(SearchText) || x.blog_Title.Contains(SearchText));
            }
            ViewData["SearchText"] = SearchText;

          //  var lst = lstSanPham.ToPagedList(pageNumber, pageSize);
            PagedList<blog> lst = new PagedList<blog>(lstSanPham, pageNumber, pageSize);
            return View(lst);
        }
        [Route("Create_Blog")]
        [HttpGet]
        public IActionResult Create_Blog() {
            return View();
        }

        [Route("Create_Blog")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create_Blog(blog blog, IFormFile anhDaiDienFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    blog.blog_Date= DateTime.Now;
                    blog.blog_Code = GenerateUniqueCode();
                    if (anhDaiDienFile != null && anhDaiDienFile.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(anhDaiDienFile.FileName);
                        string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Blog");
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            anhDaiDienFile.CopyTo(stream);
                        }
                        blog.blog_Image = uniqueFileName;
                    }
                    db.blogs.Add(blog);
                    db.SaveChanges();
                    return RedirectToAction("List_Blog", "Blog");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }
            return View(blog);
        }

        [Route("Edit_Blog")]
        [HttpGet]
        public IActionResult Edit_Blog(string blog_Code)
        {
            var blog = db.blogs.Find(blog_Code);
            return View(blog);
        }
        [Route("Edit_Blog")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit_Blog(blog blogUp, IFormFile anhDaiDienFile)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    blogUp.blog_Date = DateTime.Now;
                   // blogUp.blog_Code = db.blogs.Find(blogUp.blog_Code).blog_Code;
                    if (anhDaiDienFile != null && anhDaiDienFile.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(anhDaiDienFile.FileName);
                        string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Blog");
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            anhDaiDienFile.CopyTo(stream);
                        }
                        blogUp.blog_Image = uniqueFileName;
                    }
                    db.Entry(blogUp).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("List_Blog", "Blog");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }
            return View(blogUp);
        }
        //[HttpGet]
        //////[ValidateAntiForgeryToken]
        //[Route("Delete_Blog")]
        //public IActionResult Delete_Blog(string blog_Code)
        //{
        //    TempData["Message"] = "";


        //        var blog = db.blogs.Find(blog_Code);

        //        if (blog != null)
        //        {
        //            db.Remove(blog);
        //            db.SaveChanges();
        //            TempData["Message"] = "Blog is deleted";
        //        }
        //        else
        //        {
        //            TempData["Message"] = "Blog not found";
        //        }


        //    return RedirectToAction("List_Blog", "Blog");
        //}
        [Route("Delete_Blog")]
        [HttpGet]
        public IActionResult Delete_Blog(string blog_Code)
        {
            var blog = db.blogs.SingleOrDefault(x => x.blog_Code == blog_Code);
            ViewBag.BlogCode = blog_Code; // Pass brand code to the view
            return View(blog);
        }

        [Route("Delete_Blog")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete_BlogConfirmed(string blog_Code)
        {
            try
            {
                var blog = db.blogs.Find(blog_Code);
                db.Remove(blog);
                db.SaveChanges();
                return RedirectToAction("List_Blog", "Blog");

            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpGet]
        public IActionResult Blog_Detail(string blog_Code)
        {
            var blog = db.blogs.SingleOrDefault(x => x.blog_Code == blog_Code);
            return View(blog);
        }
        private string GenerateUniqueCode()
        {
            // Generate a random 4-digit code
            Random random = new Random();
            string code;

            do
            {
                int randomCode = random.Next(1000, 10000);
                code = randomCode.ToString("D4"); // Ensure it has 4 digits
            } while (db.blogs.Any(b => b.blog_Code == code));

            return code;
        }
    }
}
