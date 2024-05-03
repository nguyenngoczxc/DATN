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
    [Route("admin/Zipper")]
    [Authorize("Admin")]

    public class ZipperController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        private readonly DataContext db;
        public ZipperController(DataContext dba)
        {
            db = dba;

        }

        [Route("List_Zipper")]
        public IActionResult List_Zipper(string SearchText, int? page)
        {
            int pageSize = 10;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            var lstSanPham = db.zippers.AsNoTracking().OrderBy(x => x.zipper_Name).ToList();
            if (!string.IsNullOrEmpty(SearchText))
            {
                lstSanPham = lstSanPham.Where(x => x.zipper_Code.Equals(SearchText) || x.zipper_Name.Contains(SearchText)).ToList();
            }
            //var lst = lstSanPham.ToPagedList(lstSanPham,pageNumber, pageSize);
            PagedList<zipper> lst = new PagedList<zipper>(lstSanPham, pageNumber, pageSize);
            return View(lst);
        }
        [Route("Create_Zipper")]
        [HttpGet]
        public IActionResult Create_Zipper()
        {

            return View();
        }

        [Route("Create_Zipper")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create_Zipper(zipper Zipper)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    Zipper.zipper_Code = GenerateUniqueCode();
                    db.zippers.Add(Zipper);
                    db.SaveChanges();

                    return RedirectToAction("List_Zipper", "Zipper");
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi khi thêm sản phẩm hoặc tải ảnh
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            // Nếu có lỗi validation, hiển thị lại form với thông tin đã nhập
            return View(Zipper);
        }



        [Route("Edit_Zipper")]
        [HttpGet]
        public IActionResult Edit_Zipper(string zipper_Code)
        {
            var Zipper = db.zippers.SingleOrDefault(x => x.zipper_Code == zipper_Code);
            return View(Zipper);
        }

        [Route("Edit_Zipper")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit_Zipper(zipper Zipper)
        {
            if (ModelState.IsValid)
            {
                db.Entry(Zipper).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("List_Zipper", "Zipper");

            }
            return View(Zipper);
        }

        [Route("Delete_Zipper")]
        [HttpGet]
        public IActionResult Delete_Zipper(string zipper_Code)
        {
            ViewBag.ZipperCode = zipper_Code; // Pass Zipper code to the view
            ViewBag.ZipperName = db.zippers.Find(zipper_Code).zipper_Name;
            return View("Delete_Zipper");
        }

        [Route("Delete_Zipper")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete_ZipperConfirmed(string zipper_Code)
        {
            try
            {
                var Zipper = db.zippers.Find(zipper_Code);
                var products = db.products.Where(x => x.zipper_Code == Zipper.zipper_Code);
                foreach (var product in products)
                {
                    var images = db.product_Images.Where(pi => pi.product_Code == product.product_Code).ToList();
                    if (images.Any())
                    {
                        db.RemoveRange(images);
                    }
                    var prices = db.product_Sizes.Where(pi => pi.product_Code == product.product_Code).ToList();
                    if (prices.Any())
                    {
                        db.RemoveRange(prices);
                    }
                    var productcolor = db.product_Colors.Where(pi => pi.product_Code == product.product_Code).ToList();
                    if (productcolor.Any())
                    {
                        db.RemoveRange(productcolor);
                    }
                }
                if (products.Any())
                    db.RemoveRange(products);
                db.Remove(Zipper);
                db.SaveChanges();
                return RedirectToAction("List_Zipper", "Zipper");
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
            } while (db.zippers.Any(b => b.zipper_Code == code));

            return code;
        }
    }
}
