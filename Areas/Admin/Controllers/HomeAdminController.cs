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
    [Route("admin/homeadmin")]
    [Authorize(Roles = "Admin")]

    public class HomeAdminController : Controller
    {
        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeAdminController(IWebHostEnvironment webHostEnvironment, DataContext _db)
        {
            db = _db;
            _webHostEnvironment = webHostEnvironment;
        }
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Route("GetStatistical")]

        public IActionResult GetStatistical(string fromDate, string toDate)
        {
            var query = from o in db.invoices
                        join od in db.invoice_Details
                        on o.invoice_Code equals od.invoice_Code
                        join p in db.products
                        on od.product_Code equals p.product_Code
                        where o.status != 4  // Exclude invoices with status = 4
                        select new
                        {
                            CreatedDate = o.invoice_Date,
                            Quantity = od.quantity_Sold,
                            Price = od.price
                        };
            if (!string.IsNullOrEmpty(fromDate))
            {
               DateTime startDate = DateTime.ParseExact(fromDate, "yyyy-MM-dd", null);
                query = query.Where(x => x.CreatedDate >= startDate);
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                DateTime endDate = DateTime.ParseExact(toDate, "yyyy-MM-dd", null);
                query = query.Where(x => x.CreatedDate < endDate.AddDays(1)); // Add one day to include the end date
            }
            var result = query
        // Nhóm theo ngày
        .GroupBy(x => x.CreatedDate.HasValue ? x.CreatedDate.Value.Date : (DateTime?)null)
        .Select(x => new
        {
            Date1 = x.Key, // Nhóm theo ngày
            TotalSell = x.Sum(y => y.Quantity * y.Price),
        })
        // Sắp xếp theo ngày giảm dần (sử dụng kiểu DateTime để sắp xếp)
        .OrderByDescending(x => x.Date1)
        // Lấy 5 ngày gần nhất
        .Take(5)
        // Chuyển đổi `Date1` sang chuỗi "dd/MM/yyyy"
        .Select(x => new
        {
            Date = x.Date1.HasValue ? x.Date1.Value.ToString("dd/MM/yyyy") : null,
            DoanhThu = x.TotalSell,
        });

            return Json(new { Data = result });
        }
        //public IActionResult RevenueStatistics(DateTime? startDate, DateTime? endDate)
        //{
        //    // Kiểm tra ngày bắt đầu và ngày kết thúc
        //    if (startDate == null || endDate == null)
        //    {
        //        return BadRequest("Vui lòng cung cấp khoảng thời gian hợp lệ.");
        //    }

        //    // Truy vấn doanh thu trong khoảng thời gian được cung cấp
        //    var totalRevenue = db.invoices
        //        .Where(i => i.date >= startDate && i.date <= endDate)
        //        .Sum(i => i.totalAmount);

        //    // Trả về kết quả doanh thu
        //    ViewData["TotalRevenue"] = totalRevenue;
        //    ViewData["StartDate"] = startDate.Value.ToShortDateString();
        //    ViewData["EndDate"] = endDate.Value.ToShortDateString();

        //    return View();
        //}

    }
}
