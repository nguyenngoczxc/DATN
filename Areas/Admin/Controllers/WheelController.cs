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
    [Route("admin/Wheel")]
    [Authorize("Admin")]

    public class WheelController : Controller
    {
        private readonly DataContext db;
        public WheelController(DataContext dba)
        {
            db = dba;
        }

        [Route("List_Wheel")]
        public IActionResult List_Wheel(string SearchText, int? page)
        {
            int pageSize = 10;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            var lstSanPham = db.wheels.AsNoTracking().OrderBy(x => x.wheel_Name).ToList();
            if (!string.IsNullOrEmpty(SearchText))
            {
                lstSanPham = lstSanPham.Where(x => x.wheel_Code.Equals(SearchText) || x.wheel_Name.Contains(SearchText)).ToList();
            }
            //var lst = lstSanPham.ToPagedList(lstSanPham,pageNumber, pageSize);
            PagedList<wheel> lst = new PagedList<wheel>(lstSanPham, pageNumber, pageSize);
            return View(lst);
        }
        [Route("Create_Wheel")]
        [HttpGet]
        public IActionResult Create_Wheel()
        {

            return View();
        }

        [Route("Create_Wheel")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create_Wheel(wheel Wheel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Wheel.wheel_Code = GenerateUniqueCode();
                    db.wheels.Add(Wheel);
                    db.SaveChanges();

                    return RedirectToAction("List_Wheel", "Wheel");
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi khi thêm sản phẩm hoặc tải ảnh
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            // Nếu có lỗi validation, hiển thị lại form với thông tin đã nhập
            return View(Wheel);
        }



        [Route("Edit_Wheel")]
        [HttpGet]
        public IActionResult Edit_Wheel(string wheel_Code)
        {
            var Wheel = db.wheels.SingleOrDefault(x => x.wheel_Code == wheel_Code);
            return View(Wheel);
        }

        [Route("Edit_Wheel")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit_Wheel(wheel Wheel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(Wheel).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("List_Wheel", "WheelProduct");

            }
            return View(Wheel);
        }



        [Route("Delete_Wheel")]
        [HttpGet]
        public IActionResult Delete_Wheel(string wheel_Code)
        {
            ViewBag.WheelCode = wheel_Code; // Pass brand code to the view
            ViewBag.WheelName = db.wheels.Find(wheel_Code).wheel_Name;
            return View("Delete_Wheel");
        }

        [Route("Delete_Wheel")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete_WheelConfirmed(string wheel_Code)
        {
            try
            {
                var Wheel = db.wheels.Find(wheel_Code);
                var products = db.products.Where(x => x.wheel_Code == Wheel.wheel_Code);
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
                db.Remove(Wheel);
                db.SaveChanges();
                return RedirectToAction("List_Wheel", "WheelProduct");

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
            } while (db.wheels.Any(b => b.wheel_Code == code));

            return code;
        }
    }
}
