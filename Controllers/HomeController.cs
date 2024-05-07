using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using System.Diagnostics;
using TTTN3.Models;
using Azure;
using Microsoft.AspNetCore.Authorization;

namespace TTTN3.Controllers
{
    [Route("Home")]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment, DataContext _db)
        {
            _logger = logger;
            db = _db;
            _webHostEnvironment = webHostEnvironment;
        }
        [Route("Index")]
        public IActionResult Index()
        {
            var lstSanPham = db.products
                .Include(p => p.product_Sizes)
                .Include(p => p.product_Images)
                .Include(p => p.product_Colors)
                .AsNoTracking()
                .OrderByDescending(x => x.product_Sizes.Min(s => s.Price))  // Sắp xếp giảm dần theo giá
                .Take(8)
                .ToList();

            // Dictionary to store the minimum price for each product code
            Dictionary<string, decimal> minPrices = new Dictionary<string, decimal>();

            // Calculate the minimum price for each product
            foreach (var product in lstSanPham)
            {
                decimal minPrice = decimal.MaxValue;

                if (product.product_Sizes != null && product.product_Sizes.Any())
                {
                    minPrice = product.product_Sizes.Min(s => s.Price);
                }

                minPrices.Add(product.product_Code, minPrice);
            }

            // Pass the dictionary to the ViewBag
            ViewBag.MinPrices = minPrices;

            var materials = db.materials.ToList(); // Thay thế bằng logic lấy danh sách chất liệu của bạn

            var viewModel = new ProductListViewModel
            {
                Products = lstSanPham,
                Materials = materials,
                MinPrices = minPrices
            };

            return View(viewModel);
        }
        //public IActionResult Index()
        //{
        //    // Step 1: Calculate total quantity sold for each product
        //    var totalSoldQuery = from od in db.invoice_Details
        //                         group od by od.product_Code into g
        //                         select new
        //                         {
        //                             product_Code = g.Key,
        //                             TotalSold = g.Sum(od => od.quantity_Sold)
        //                         };

        //    // Step 2: Order by total quantity sold in descending order and take the top 8 best-selling products
        //    var topSellingProducts = totalSoldQuery
        //        .OrderByDescending(x => x.TotalSold)
        //        .Take(8)
        //        .ToList();

        //    // Step 3: Load products with associated entities for the top-selling products
        //    var productCodes = topSellingProducts.Select(x => x.product_Code).ToList();
        //    var bestSellingProducts = db.products
        //        .Where(p => productCodes.Contains(p.product_Code))
        //        .Include(p => p.product_Sizes)
        //        .Include(p => p.product_Images)
        //        .Include(p => p.product_Colors)
        //        .AsNoTracking()
        //        .ToList();

        //    // Step 4: Calculate minimum prices for the best-selling products
        //    Dictionary<string, decimal> minPrices = new Dictionary<string, decimal>();
        //    foreach (var product in bestSellingProducts)
        //    {
        //        if (product.product_Sizes != null && product.product_Sizes.Any())
        //        {
        //            decimal minPrice = product.product_Sizes.Min(s => s.Price);
        //            minPrices[product.product_Code] = minPrice;
        //        }
        //    }

        //    // Step 5: Load materials list (replace with your logic to get the list of materials)
        //    ViewBag.MinPrices = minPrices;
        //    var materials = db.materials.ToList();

        //    // Step 6: Create the view model and return the view
        //    var viewModel = new ProductListViewModel
        //    {
        //        Products = bestSellingProducts,
        //        Materials = materials,
        //        MinPrices = minPrices
        //    };

        //    return View(viewModel);
        //}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
