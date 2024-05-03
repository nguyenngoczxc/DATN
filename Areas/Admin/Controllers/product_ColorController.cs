using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
//using System.Web.Mvc;
using TTTN3.Models;
using X.PagedList;

namespace TTTN3.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin")]
    [Route("admin/product_Color")]
    [Authorize("Admin")]

    public class product_ColorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        private readonly DataContext db;
        public product_ColorController(DataContext dba)
        {
            db = dba;

        }

        [Route("List_Product_Color")]
        public IActionResult List_Product_Color(string? SearchText, int? page)
        {

            int pageSize = 10;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            IQueryable<product_Color> lstSanPham = db.product_Colors
                .Include(x => x.product)  // Include the related product information
                .Include(x => x.color)
                .AsNoTracking()
                .OrderBy(x => x.product.product_Name);
            if (!string.IsNullOrEmpty(SearchText))
            {
                lstSanPham = lstSanPham.Where(x => x.product.product_Name.Equals(SearchText) || x.color.color_Name.Contains(SearchText));
            }
            ViewData["SearchText"] = SearchText;

            //var lst = lstSanPham.ToPagedList(lstSanPham,pageNumber, pageSize);
            PagedList<product_Color> lst = new PagedList<product_Color>(lstSanPham, pageNumber, pageSize);
            return View(lst);
        }

        [Route("Create_Product_Color")]
        [HttpGet]
        public IActionResult Create_Product_Color()
        {
            ViewBag.product_Code = new SelectList(db.products.ToList(),
                "product_Code", "product_Name");
            ViewBag.color_Code = new SelectList(db.colors.ToList(),
                "color_Code", "color_Name");
            return View();
        }

        [Route("Create_Product_Color")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create_Product_Color(product_Color product_Color)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    product_Color.product_Color_Code = GenerateUniqueCode();
                    db.product_Colors.Add(product_Color);
                    db.SaveChanges();
                    return RedirectToAction("List_Product_Color", "product_Color");
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi khi thêm sản phẩm hoặc tải ảnh
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            // Nếu có lỗi validation, hiển thị lại form với thông tin đã nhập
            return View(product_Color);
        }



        [Route("Edit_Product_Color")]
        [HttpGet]
        public IActionResult Edit_Product_Color(string product_Color_Code)
        {
            var product_Color = db.product_Colors.SingleOrDefault(x => x.product_Color_Code == product_Color_Code);
            ViewBag.product_Code = new SelectList(db.products.ToList(),
                "product_Code", "product_Name");
            ViewBag.color_Code = new SelectList(db.colors.ToList(),
                "color_Code", "color_Name");
            return View(product_Color);
        }

        [Route("Edit_Product_Color")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit_Product_Color(product_Color product_Color)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product_Color).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("List_Product_Color", "product_Color");

            }
            return View(product_Color);
        }

        [Route("Delete_Product_Color")]
        [HttpGet]
        public IActionResult Delete_Product_Color(string product_Color_Code)
        {
            ViewBag.ProductColorCode = product_Color_Code; // Pass brand code to the view
            ViewBag.ProductName = db.product_Colors.Find(product_Color_Code).product.product_Name;
            return View("Delete_Product_Color");
        }

        [Route("Delete_Product_Color")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete_Product_ColorConfirmed(string product_Color_Code)
        {
            try
            {
                var product_Color = db.product_Colors.Find(product_Color_Code);
                db.Remove(product_Color);
                db.SaveChanges();
                return RedirectToAction("List_Product_Color", "product_Color");
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
            } while (db.product_Colors.Any(b => b.product_Color_Code == code));

            return code;
        }
    }
}
