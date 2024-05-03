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
    [Route("admin/product")]
    [Authorize("Admin")]

    public class ProductController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly DataContext db;
        public ProductController(IWebHostEnvironment webHostEnvironment, DataContext dba)
        {
            db = dba;

            _webHostEnvironment = webHostEnvironment;
        }
        //DataContext db = new DataContext();
        [Route("")]
        [Route("index")]
        public IActionResult Index()
        {

            return View();
        }
        [Route("List_Product")]
        public IActionResult List_Product(string SearchText, int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page < 1 ? 1 : page.Value;

            IQueryable<product> query = db.products
                .Include(x => x.brand)
                .Include(x => x.material)
                .Include(x => x.wheel)
                .Include(x => x.zipper)
                .AsNoTracking()
                .OrderBy(x => x.product_Name);

            if (!string.IsNullOrEmpty(SearchText))
            {
                // Kiểm tra nếu SearchText có thể là một số
                if (int.TryParse(SearchText, out int searchQuantity))
                {
                    // Tìm kiếm theo quantity
                    query = query.Where(x => x.quantity == searchQuantity);
                }
                else
                {
                    query = query
                    .Where(x => x.product_Code.Equals(SearchText) ||
                                x.product_Name.Contains(SearchText) ||
                                x.brand.brand_Name.Contains(SearchText) ||
                                x.wheel.wheel_Name.Contains(SearchText) ||
                                x.zipper.zipper_Name.Contains(SearchText) ||
                                x.material.material_Name.Contains(SearchText));
                }
            }

            PagedList<product> lst = new PagedList<product>(query, pageNumber, pageSize);
            ViewData["SearchText"] = SearchText;

            return View(lst);
        }
        [Route("Create_Product")]
        [HttpGet]
        public IActionResult Create_Product()
        {
            ViewBag.brand_Code = new SelectList(db.brands.ToList(),
                "brand_Code", "brand_Name");
            ViewBag.material_Code = new SelectList(db.materials.ToList(),
                "material_Code", "material_Name");
            ViewBag.wheel_Code = new SelectList(db.wheels.ToList(),
                "wheel_Code", "wheel_Name");
            ViewBag.zipper_Code = new SelectList(db.zippers.ToList(),
                "zipper_Code", "zipper_Name");
            ViewBag.Colors = new SelectList(db.colors.ToList(),
                "color_Code", "color_Name");
            return View();
        }

        [Route("Create_Product")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create_Product(product product, IFormFile anhDaiDienFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    product.product_Code = GenerateUniqueCode();
                    if (anhDaiDienFile != null && anhDaiDienFile.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(anhDaiDienFile.FileName);
                        string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Product");
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            anhDaiDienFile.CopyTo(stream);
                        }

                        product.product_Avatar = uniqueFileName;
                    }

                    db.products.Add(product);
                    db.SaveChanges();
                    return RedirectToAction("List_Product", "Product");
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi khi thêm sản phẩm hoặc tải ảnh
                    ModelState.AddModelError("", "Có lỗi khi thêm sản phẩm: " + ex.Message);
                }
            }

            // Nếu có lỗi validation, hiển thị lại form với thông tin đã nhập
            return View(product);
        }



        [Route("Edit_Product")]
        [HttpGet]
        public IActionResult Edit_Product(string product_Code)
        {
            ViewBag.brand_Code = new SelectList(db.brands.ToList(),
               "brand_Code", "brand_Name");
            ViewBag.material_Code = new SelectList(db.materials.ToList(),
                "material_Code", "material_Name");
            ViewBag.wheel_Code = new SelectList(db.wheels.ToList(),
                "wheel_Code", "wheel_Name");
            ViewBag.zipper_Code = new SelectList(db.zippers.ToList(),
                "zipper_Code", "zipper_Name");
            var product = db.products.Find(product_Code);
            return View(product);
        }

        [Route("Edit_Product")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit_Product(product product, IFormFile anhDaiDienFile)
        {
            var existing = db.products.Find(product.product_Code);
                if (anhDaiDienFile != null && anhDaiDienFile.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(anhDaiDienFile.FileName);
                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Product");
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        anhDaiDienFile.CopyTo(stream);
                    }

                    existing.product_Avatar = uniqueFileName;
                }
            else
            {
                // Không có ảnh mới, giữ nguyên giá trị hiện tại của `promotion_Image`
                existing.product_Avatar = existing.product_Avatar;
            }
                existing.product_Name = product.product_Name;
            existing.product_Name = product.product_Name;
            existing.brand_Code = product.brand_Code;
            existing.material_Code = product.material_Code;
            existing.zipper_Code = product.zipper_Code;
            existing.wheel_Code = product.wheel_Code;
            existing.quantity = product.quantity;
            existing.weight = product.weight;
            existing.description = product.description;

            //db.Entry(product).State = EntityState.Modified;
            //db.Update(product);
            db.SaveChanges();
            return RedirectToAction("List_Product", "Product");
            
        }
        [Route("Delete_Product")]
        [HttpGet]
        public IActionResult Delete_Product(string product_Code)
        {
            var product = db.products.SingleOrDefault(x => x.product_Code == product_Code);
            ViewBag.ProductCode = product_Code; // Pass brand code to the view
            ViewBag.ProductName = db.products.Find(product_Code).product_Name;
            return View(product);
        }
        [Route("Delete_Product")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete_ProductConfirmed(string product_Code)
        {
            TempData["Message"] = "";
            //var chiTietproducts = db.TChiTietproducts.Where(x => x.MaSp == maSanPham).ToList();
            //if (chiTietproducts.Count() > 0)
            //{
            //    TempData["Message"] = "Khong xoa duoc san pham nay";
            //    return RedirectToAction("DanhMucSanPham", "HomeAdmin");
            //}
            var anhproducts = db.product_Images.Where(x => x.product_Code == product_Code);
            if (anhproducts.Any())
                db.RemoveRange(anhproducts);
            var Product_Colors = db.product_Colors.Where(x => x.product_Code == product_Code);
            if (Product_Colors.Any())
                db.RemoveRange(Product_Colors);
            var Product_Sizes = db.product_Sizes.Where(x => x.product_Code == product_Code);
            if (Product_Sizes.Any())
                db.RemoveRange(Product_Sizes);
            var Favorite_Product = db.FavoriteProduct.Where(x => x.ProductId == product_Code);
            if (Favorite_Product.Any())
                db.RemoveRange(Favorite_Product);
            db.Remove(db.products.Find(product_Code));
            db.SaveChanges();
            TempData["Message"] = "Product has been removed";
            return RedirectToAction("List_Product", "Product");
        }
        [HttpGet]
        [Route("Product_Detail")]

        public IActionResult Product_Detail(string product_Code)
        {
            var product = db.products.SingleOrDefault(x => x.product_Code == product_Code);
            var anhSanPham = db.product_Images.Where(x => x.product_Code == product_Code).ToList();
            var Product_Colors = db.product_Colors.Where(x => x.product_Code == product_Code).ToList();
            var Product_Sizes = db.product_Sizes.Where(x => x.product_Code == product_Code).ToList();
            ViewBag.anhSanPham = anhSanPham;
            ViewBag.brand = db.brands.FirstOrDefault(x => x.brand_Code == product.brand_Code).brand_Name;
            ViewBag.material = db.materials.FirstOrDefault(x => x.material_Code == product.material_Code).material_Name;
            ViewBag.wheel = db.wheels.FirstOrDefault(x => x.wheel_Code == product.wheel_Code).wheel_Name;
            ViewBag.zipper = db.zippers.FirstOrDefault(x => x.zipper_Code == product.zipper_Code).zipper_Name;
            return View(product);
        }
        private string GenerateUniqueCode()
        {
            Random random = new Random();
            string code;

            do
            {
                int randomCode = random.Next(1000, 10000);
                code = randomCode.ToString("D4"); // Ensure it has 4 digits
            } while (db.products.Any(b => b.product_Code == code));

            return code;
        }
        private string GetUniqueFileName(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            string uniqueFileName = name + "_" + Guid.NewGuid().ToString("N") + extension;
            return uniqueFileName;
        }
        [HttpPost]
        [Route("ApplyKhuyenMai")]
        public JsonResult ApplyKhuyenMai(bool applyKhuyenMai)
        {
            if (applyKhuyenMai)
            {
                // Thay đổi view_Count của tất cả các sản phẩm thành 1
                var products = db.products.ToList(); // Lấy tất cả sản phẩm từ cơ sở dữ liệu
                var check = 0;
                foreach (var product in products)
                {
                    product.sale = !product.sale;
                    if (product.sale == true) check = 1;
                }

                // Lấy tất cả các bản ghi trong bảng product_Size
                var productSizes = db.product_Sizes.ToList();

                // Thay đổi giá trị price trong bảng product_Size
                decimal discountRate = (decimal)0.9;

                // Thay đổi giá trị price trong bảng product_Size
                foreach (var productSize in productSizes)
                {
                    if (check == 1)
                    {
                        productSize.Price = productSize.Price * discountRate; // Giảm giá 10%
                    }
                    else
                    {
                        productSize.Price = productSize.Price / discountRate; // hoàn lại giá
                    }
                }

                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }

    }
}