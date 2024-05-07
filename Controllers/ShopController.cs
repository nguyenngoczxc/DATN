using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using System.Diagnostics;
using TTTN3.Models;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using TTTN3.Models.ViewModels;
using Azure;
using System.Security.Claims;
using System.Drawing.Printing;



namespace TTTN3.Controllers
{
    [Route("Shop")]
    [AllowAnonymous]
    public class ShopController : Controller
    {
        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ShopController> _logger;

        public ShopController(ILogger<ShopController> logger, IWebHostEnvironment webHostEnvironment, DataContext _db)
        {
            _logger = logger;
            db = _db;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        [Route("Index")]
        public IActionResult Index(string[] selectedMaterials, string[] selectedBrands, decimal minPrice, decimal maxPrice, string sortOption, string searchTerm, int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            var query = db.products
                .Include(p => p.product_Sizes)
                .Include(p => p.product_Images)
                .Include(p => p.product_Colors)
                .AsNoTracking();

            // Apply filters based on user input
            if (selectedMaterials != null && selectedMaterials.Length > 0)
            {
                query = query.Where(x => selectedMaterials.Contains(x.material_Code));
            }

            if (selectedBrands != null && selectedBrands.Length > 0)
            {
                query = query.Where(x => selectedBrands.Contains(x.brand_Code));
            }

            if (minPrice >= 0 && maxPrice > 0)
            {
                query = query.Where(x => x.product_Sizes.Any(s => s.Price >= minPrice && s.Price <= maxPrice));
            }
            if (searchTerm != null)
            {
                query = query.Where(p =>
                    p.product_Name.Contains(searchTerm) ||
                    p.material.material_Name.Contains(searchTerm) ||
                    p.brand.brand_Name.Contains(searchTerm) ||
                    p.wheel.wheel_Name.Contains(searchTerm) ||
                    p.zipper.zipper_Name.Contains(searchTerm)
                );
            }
            var lstSanPham = query.OrderBy(p => p.product_Name).ToList();
            Dictionary<string, decimal> minPrices = new Dictionary<string, decimal>();

            // Calculate the minimum price for each product
            foreach (var product in query)
            {
                decimal minPric= decimal.MaxValue;

                if (product.product_Sizes != null && product.product_Sizes.Any())
                {
                    minPric = product.product_Sizes.Min(s => s.Price);
                }

                minPrices.Add(product.product_Code, minPric);
            }

            // Order the products based on the selected sort option
            switch (sortOption)
            {
                case "name":
                    lstSanPham = lstSanPham.OrderBy(p => p.product_Name).ToList();
                    break;
                case "price":
                    // Add sorting logic based on price (you can modify this)
                    lstSanPham = lstSanPham.OrderBy(p => minPrices[p.product_Code]).ToList();
                    break;
                default:
                    // Default sorting option
                    break;
            }

            // Pass necessary data to the ViewBag
            ViewBag.MinPrices = minPrices;
            ViewBag.Brands = db.brands.ToList();
            ViewBag.Materials = db.materials.ToList();
            ViewBag.selectedMaterials = selectedMaterials;
           ViewBag.selectedBrands = selectedBrands;
           ViewBag.minPrice = minPrice;
            ViewBag.maxPrice = maxPrice;
            ViewBag.sortOption= sortOption;
            ViewBag.searchTerm= searchTerm;
            // Check if the request is made through Ajax
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // If it's an Ajax request, return the filtered product list as a partial view
                var filteredProducts = lstSanPham
                    .Where(p =>
                        (selectedMaterials == null || selectedMaterials.Length == 0 || selectedMaterials.Contains(p.material_Code)) &&
                        (selectedBrands == null || selectedBrands.Length == 0 || selectedBrands.Contains(p.brand_Code)) &&
                        (minPrice <= 0 || minPrices[p.product_Code] >= minPrice) &&
                        (maxPrice <= 0 || minPrices[p.product_Code] <= maxPrice))
                    .ToList();

                PagedList<product> lst = new PagedList<product>(filteredProducts, pageNumber, pageSize);

                return PartialView("_Product_List_Partial", lst);
            }
            else
            {
                // If it's not an Ajax request, return the full view with the initial product list
                PagedList<product> lst = new PagedList<product>(lstSanPham, pageNumber, pageSize);

                return View(lst);
            }
        }


        [Route("Product_Details")]

        public IActionResult Product_Details(string product_Code)
        {
            var product = db.products.SingleOrDefault(x => x.product_Code == product_Code);
            var productImage = db.product_Images.Where(x => x.product_Code == product_Code).ToList();
            var productColor = db.product_Colors.Include(i => i.color).Where(x => x.product_Code == product_Code).ToList();
            var productSize = db.product_Sizes.Include(x => x.size).Where(x => x.product_Code == product_Code).ToList();

            // Calculate the minimum price for the product
            decimal minPrice = decimal.MaxValue;

            if (productSize != null && productSize.Any())
            {
                minPrice = productSize.Min(s => s.Price);
            }

            ViewBag.MinPrice = minPrice;
            ViewBag.productImage = productImage;
            ViewBag.productColor = productColor;
            ViewBag.productSize = productSize;
            ViewBag.ProductCode = product_Code;
            ViewBag.Wheel = db.wheels.SingleOrDefault(b => b.wheel_Code == product.wheel_Code).wheel_Name;
            ViewBag.Zipper = db.zippers.SingleOrDefault(b => b.zipper_Code == product.zipper_Code).zipper_Name;
            var material = db.materials.SingleOrDefault(b => b.material_Code == product.material_Code).material_Name;
            ViewBag.Material = material;
            return View(product);

        }

        [Authorize]
        [HttpPost]
        [Route("AddToFavorites")]
        public IActionResult AddToFavorites(string productId)
        {
            // Assume you have user authentication, and you can get the user's ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
            {
                var existingFavorite = db.FavoriteProduct
                    .FirstOrDefault(fp => fp.UserId == userId && fp.ProductId == productId);

                if (existingFavorite == null)
                {
                    var newFavorite = new FavoriteProduct
                    {
                        Id= GenerateUniqueCode(),
                        UserId = userId,
                        ProductId = productId,
                    };

                    db.FavoriteProduct.Add(newFavorite);
                    db.SaveChanges();

                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
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
            } while (db.FavoriteProduct.Any(b => b.Id == code)) ;

            return code;
        }
        [Authorize]
        [Route("FavoriteProducts")]
        public IActionResult FavoriteProducts(int? page)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var pageNumber = page ?? 1;
                var pageSize = 12; // Số sản phẩm trên mỗi trang, bạn có thể điều chỉnh theo ý muốn

                var favoriteProducts = db.FavoriteProduct
                    .Where(fp => fp.UserId == userId)
                    .Include(fp => fp.product)
                    .ToList();
                // Tính toán số lượng sản phẩm yêu thích
                int favoriteProductCount = favoriteProducts.Count;

                // Gán giá trị vào ViewBag
                ViewData["favoriteProductCount"] = favoriteProductCount;
                PagedList<FavoriteProduct> lst = new PagedList<FavoriteProduct>(favoriteProducts, pageNumber, pageSize);
                return View(lst);
            }

            // Xử lý nếu không có người dùng đăng nhập
            return RedirectToAction("Login", "Account");
        }
        [Authorize]
        [HttpPost]
        [Route("RemoveFromFavorites")]
        public IActionResult RemoveFromFavorites(string productCode, int? page)
        {
            // Assume you have user authentication, and you can get the user's ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
            {
                // Find the favorite product
                var favoriteProduct = db.FavoriteProduct
                    .FirstOrDefault(fp => fp.UserId == userId && fp.ProductId == productCode);

                if (favoriteProduct != null)
                {
                    // Remove the favorite product
                    db.FavoriteProduct.Remove(favoriteProduct);
                    db.SaveChanges();

                    // Redirect to the FavoriteProducts page
                    return RedirectToAction("FavoriteProducts", new { page });
                }
            }

            // Handle if the user is not logged in or the favorite product is not found
            return RedirectToAction("Login", "Account");
        }

    }
}


