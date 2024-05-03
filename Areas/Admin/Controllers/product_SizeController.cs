
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
    [Route("admin/product_Size")]
    [Authorize("Admin")]

    public class product_SizeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        private readonly DataContext db;
        public product_SizeController(DataContext dba)
        {
            db = dba;

        }

        [Route("List_Product_Size")]
        public IActionResult List_Product_Size(string? SearchText,int? page)
        {

            int pageSize = 10;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            IQueryable<product_Size> lstSanPham = db.product_Sizes
                .Include(x => x.product)  // Include the related product information
                .Include(x => x.size)
                .AsNoTracking()
                .OrderBy(x => x.product.product_Name);
            if (!string.IsNullOrEmpty(SearchText))
            {
                lstSanPham = lstSanPham.Where(x => x.product.product_Name.Equals(SearchText) || x.size.size_Name.Contains(SearchText));
            }
            ViewData["SearchText"] = SearchText;
            //var lst = lstSanPham.ToPagedList(lstSanPham,pageNumber, pageSize);
            PagedList<product_Size> lst = new PagedList<product_Size>(lstSanPham, pageNumber, pageSize);
            return View(lst);
        }
        [Route("Create_Product_Size")]
        [HttpGet]
        public IActionResult Create_Product_Size()
        {
            ViewBag.product_Code = new SelectList(db.products.ToList(),
                "product_Code", "product_Name");
            ViewBag.size_Code = new SelectList(db.sizes.ToList(),
                "size_Code", "size_Name");
            return View();
        }

        [Route("Create_Product_Size")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create_Product_Size(product_Size product_Size)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    product_Size.product_Size_Code = GenerateUniqueCode();
                    db.product_Sizes.Add(product_Size);
                    db.SaveChanges();
                    return RedirectToAction("List_Product_Size", "product_Size");
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi khi thêm sản phẩm hoặc tải ảnh
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            // Nếu có lỗi validation, hiển thị lại form với thông tin đã nhập
            return View(product_Size);
        }



        [Route("Edit_Product_Size")]
        [HttpGet]
        public IActionResult Edit_Product_Size(string product_Size_Code)
        {
            var product_Size = db.product_Sizes.SingleOrDefault(x => x.product_Size_Code == product_Size_Code);
            ViewBag.product_Code = new SelectList(db.products.ToList(),
                "product_Code", "product_Name");
            ViewBag.color_Code = new SelectList(db.colors.ToList(),
                "color_Code", "color_Name");
            return View(product_Size);
        }

        [Route("Edit_Product_Size")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit_Product_Size(product_Size product_Size)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product_Size).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("List_Product_Size", "product_Size");

            }
            return View(product_Size);
        }

        [Route("Delete_Product_Size")]
        [HttpGet]
        public IActionResult Delete_Product_Size(string product_Size_Code)
        {
            ViewBag.ProductSizeCode = product_Size_Code; // Pass brand code to the view
            ViewBag.ProductName = db.product_Sizes.Find(product_Size_Code).product.product_Name;
            return View("Delete_Product_Size");
        }

        [Route("Delete_Product_Size")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete_Product_SizeConfirmed(string product_Size_Code)
        {
            try
            {
                var product_Size = db.product_Sizes.Find(product_Size_Code);
                db.Remove(product_Size);
                db.SaveChanges();
                return RedirectToAction("List_Product_Size", "product_Size");
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
            } while (db.product_Sizes.Any(b => b.product_Size_Code == code));

            return code;
        }
    }
}
