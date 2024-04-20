using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using TTTN3.Models;
using X.PagedList;

namespace TTTN3.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin")]
    [Route("admin/Color")]
    [Authorize("Admin")]

    public class ColorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        private readonly DataContext db;
        public ColorController(DataContext dba)
        {
            db = dba;

        }

        [Route("List_Color")]
        public IActionResult List_Color(string SearchText, int? page)
        {
            int pageSize = 10;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            var lstSanPham = db.colors.AsNoTracking().OrderBy(x => x.color_Name).ToList();
            if (!string.IsNullOrEmpty(SearchText))
            {
                lstSanPham = lstSanPham.Where(x => x.color_Code.Equals(SearchText) || x.color_Name.Contains(SearchText)).ToList();
            }
            ViewData["SearchText"] = SearchText;
            //var lst = lstSanPham.ToPagedList(lstSanPham,pageNumber, pageSize);
            PagedList<color> lst = new PagedList<color>(lstSanPham, pageNumber, pageSize);
            return View(lst);
        }
        [Route("Create_Color")]
        [HttpGet]
        public IActionResult Create_Color()
        {

            return View();
        }

        [Route("Create_Color")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create_Color(color Color)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    Color.color_Code = GenerateUniqueCode();
                    db.colors.Add(Color);
                    db.SaveChanges();

                    return RedirectToAction("List_Color", "Color");
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi khi thêm sản phẩm hoặc tải ảnh
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            // Nếu có lỗi validation, hiển thị lại form với thông tin đã nhập
            return View(Color);
        }



        [Route("Edit_Color")]
        [HttpGet]
        public IActionResult Edit_Color(string color_Code)
        {
            var Color = db.colors.SingleOrDefault(x => x.color_Code == color_Code);
            return View(Color);
        }

        [Route("Edit_Color")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit_Color(Color Color)
        {
            if (ModelState.IsValid)
            {
                db.Entry(Color).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("List_Color", "Color");

            }
            return View(Color);
        }

        [Route("Delete_Color")]
        [HttpGet]
        public IActionResult Delete_Color(string color_Code)
        {
            ViewBag.ColorCode = color_Code; // Pass Color code to the view
            ViewBag.ColorName = db.colors.Find(color_Code).color_Name;
            return View("Delete_Color");
        }

        [Route("Delete_Color")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete_ColorConfirmed(string color_Code)
        {
            try
            {
                var Color = db.colors.Find(color_Code);
                var product_Colors = db.product_Colors.Where(x => x.color_Code == Color.color_Code);
                        db.RemoveRange(product_Colors);
                db.Remove(Color);
                db.SaveChanges();
                return RedirectToAction("List_Color", "Color");
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
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
            } while (db.colors.Any(b => b.color_Code == code));

            return code;
        }
    }
}
