using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTTN3.Models;
using X.PagedList;

namespace TTTN3.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin")]
    [Route("admin/Size")]
    [Authorize("Admin")]

    public class SizeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        private readonly DataContext db;
        public SizeController(DataContext dba)
        {
            db = dba;

        }

        [Route("List_Size")]
        public IActionResult List_Size(string SearchText)
        {
            var lstSanPham = db.sizes.AsNoTracking().OrderBy(x => x.size_Name).ToList();
            return View(lstSanPham);
        }
        [Route("Create_Size")]
        [HttpGet]
        public IActionResult Create_Size()
        {

            return View();
        }

        [Route("Create_Size")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create_Size(size size)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    size.size_Code = GenerateUniqueCode();
                    db.sizes.Add(size);
                    db.SaveChanges();

                    return RedirectToAction("List_Size", "Size");
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi khi thêm sản phẩm hoặc tải ảnh
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            // Nếu có lỗi validation, hiển thị lại form với thông tin đã nhập
            return View(size);
        }



        [Route("Edit_Size")]
        [HttpGet]
        public IActionResult Edit_Size(string size_Code)
        {
            var size = db.sizes.SingleOrDefault(x => x.size_Code == size_Code);
            return View(size);
        }

        [Route("Edit_Size")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit_Size(size size)
        {
            if (ModelState.IsValid)
            {
                db.Entry(size).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("List_Size", "Size");

            }
            return View(size);
        }

        [Route("Delete_Size")]
        [HttpGet]
        public IActionResult Delete_Size(string size_Code)
        {
            ViewBag.SizeCode = size_Code; // Pass brand code to the view
            ViewBag.SizeName = db.sizes.Find(size_Code).size_Name;
            return View("Delete_Size");
        }

        [Route("Delete_Size")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete_SizeConfirmed(string size_Code)
        {
            try
            {
                var productSizes= db.sizes.Where(x => x.size_Code == size_Code);
                if (productSizes.Any()) 
                    db.RemoveRange(productSizes);
                db.Remove(db.sizes.Find(size_Code));
                db.SaveChanges();
                return RedirectToAction("List_Size", "Size");
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
            } while (db.sizes.Any(b => b.size_Code == code));

            return code;
        }
    }
}