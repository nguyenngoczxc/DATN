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
    [Authorize("Admin")]

    [Route("admin/Brand")]
    public class BrandController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        private readonly DataContext db;
        public BrandController(DataContext dba)
        {
            db = dba;

        }

        [Route("List_Brand")]
        public IActionResult List_Brand(string SearchText, int? page)
        {
            int pageSize = 10;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            IQueryable<brand> lstSanPham = db.brands.AsNoTracking().OrderBy(x => x.brand_Name);
            if (!string.IsNullOrEmpty(SearchText))
            {
                lstSanPham = lstSanPham.Where(x => x.brand_Code.Equals(SearchText) || x.brand_Name.Contains(SearchText));
            }
            ViewData["SearchText"] = SearchText;
            //var lst = lstSanPham.ToPagedList(lstSanPham,pageNumber, pageSize);
            PagedList<brand> lst = new PagedList<brand>(lstSanPham, pageNumber, pageSize);
            return View(lst);
        }
        [Route("Create_Brand")]
        [HttpGet]
        public IActionResult Create_Brand()
        {

            return View();
        }

        [Route("Create_Brand")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create_Brand(brand brand)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    brand.brand_Code = GenerateUniqueCode();
                    db.brands.Add(brand);
                    db.SaveChanges();

                    return RedirectToAction("List_Brand", "Brand");
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi khi thêm sản phẩm hoặc tải ảnh
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            // Nếu có lỗi validation, hiển thị lại form với thông tin đã nhập
            return View(brand);
        }



        [Route("Edit_Brand")]
        [HttpGet]
        public IActionResult Edit_Brand(string brand_Code)
        {
            var brand = db.brands.SingleOrDefault(x => x.brand_Code == brand_Code);
            return View(brand);
        }

        [Route("Edit_Brand")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit_Brand(brand brand)
        {
            if (ModelState.IsValid)
            {
                db.Entry(brand).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("List_Brand", "Brand");

            }
            return View(brand);
        }

        [Route("Delete_Brand")]
        [HttpGet]
        public IActionResult Delete_Brand(string brand_Code)
        {
            ViewBag.BrandCode = brand_Code; // Pass brand code to the view
            ViewBag.BrandName = db.brands.Find(brand_Code).brand_Name;
            return View("Delete_Brand");
        }

        [Route("Delete_Brand")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete_BrandConfirmed(string brand_Code)
        {
            try
            {
                var brand = db.brands.Find(brand_Code); 
                var products = db.products.Where(x => x.brand_Code == brand.brand_Code);
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
                db.Remove(brand);
                db.SaveChanges();
                return RedirectToAction("List_Brand", "Brand");
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
            } while (db.brands.Any(b => b.brand_Code == code));

            return code;
        }
    }
}
