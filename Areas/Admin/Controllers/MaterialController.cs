using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTTN3.Models;
using X.PagedList;

namespace TTTN3.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Route("admin")]
    [Route("admin/Material")]
    [Authorize("Admin")]

    public class MaterialController : Controller
    {
        private readonly DataContext db;
        public MaterialController(DataContext dba)
        {
            db = dba;

        }
        
        [Route("List_Material")]
        public IActionResult List_Material(string SearchText, int? page)
        {
            int pageSize = 10;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            IQueryable<material> lstSanPham = db.materials.AsNoTracking().OrderBy(x => x.material_Name);
            if (!string.IsNullOrEmpty(SearchText))
            {
                lstSanPham = lstSanPham.Where(x => x.material_Code.Equals(SearchText) || x.material_Name.Contains(SearchText));
            }
            ViewData["SearchText"] = SearchText;

            //var lst = lstSanPham.ToPagedList(lstSanPham,pageNumber, pageSize);
            PagedList<material> lst = new PagedList<material>(lstSanPham, pageNumber, pageSize);
            return View(lst);
        }
        [Route("Create_Material")]
        [HttpGet]
        public IActionResult Create_Material()
        {

            return View();
        }

        [Route("Create_Material")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create_Material(material material)
        {
            if (ModelState.IsValid)
            {
                try
                {
                   material.material_Code = GenerateUniqueCode();
                    db.materials.Add(material);
                    db.SaveChanges();

                    return RedirectToAction("List_Material", "Material");
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi khi thêm sản phẩm hoặc tải ảnh
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            // Nếu có lỗi validation, hiển thị lại form với thông tin đã nhập
            return View(material);
        }



        [Route("Edit_Material")]
        [HttpGet]
        public IActionResult Edit_Material(string material_Code)
        {
            var material = db.materials.SingleOrDefault(x => x.material_Code == material_Code);
            return View(material);
        }

        [Route("Edit_Material")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit_Material(material material)
        {
            if (ModelState.IsValid)
            {
                db.Entry(material).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("List_Material", "Material");

            }
            return View(material);
        }



        [Route("Delete_Material")]
        [HttpGet]
        public IActionResult Delete_Material(string material_Code)
        {
            ViewBag.materialCode = material_Code; // Pass brand code to the view
            ViewBag.materialName = db.materials.Find(material_Code).material_Name;
            return View("Delete_Material");
        }

        [Route("Delete_Material")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete_MaterialConfirmed(string material_Code)
        {
            try
            {
                var material = db.materials.Find(material_Code);
                var products = db.products.Where(x => x.material_Code == material.material_Code);
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
                db.Remove(material);
                db.SaveChanges();
                return RedirectToAction("List_Material", "Material");

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
            } while (db.materials.Any(b => b.material_Code == code));

            return code;
        }
    }
}
