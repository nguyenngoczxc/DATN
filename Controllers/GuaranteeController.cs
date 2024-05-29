using Microsoft.AspNetCore.Mvc;
using TTTN3.Models;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TTTN3.Controllers
{
    public class GuaranteeController : Controller
    {
        private UserManager<AppUserModel> _userManager;
        private readonly DataContext _context;

        public GuaranteeController(DataContext context, UserManager<AppUserModel> userManager)
        {
            _context = context;
            _userManager = userManager;

        }

        // Hiển thị form nhập thông tin bảo hành
        public IActionResult Guarantee_Invoice(string invoice_Detail_Code)
        {
            var invoiceDetail = _context.invoice_Details.Include(x => x.product).FirstOrDefault(i => i.invoice_Detail_Code == invoice_Detail_Code);
            if (invoiceDetail == null)
            {
                return NotFound();
            }

            var model = new guarantee { invoice_Detail_Code = invoice_Detail_Code };
            string userName = User.Identity.Name;
            ViewBag.UserName = userName;
            ViewBag.Product = invoiceDetail.product.product_Name;
            ViewBag.UserId = _userManager.GetUserId(User);
            
            return View(model);
        }

        // Xử lý dữ liệu từ form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateGuarantee(guarantee guarantee)
        {
            if (ModelState.IsValid)
            {
                guarantee.guarantee_Date = DateTime.Now;
                guarantee.guarantee_Code = GenerateUniqueCode();
                guarantee.status = 1;
                guarantee.guarantee_Solution = "0";
                _context.guarantee.Add(guarantee);
                _context.SaveChanges();
                string userId = _userManager.GetUserId(User);
                return RedirectToAction("List_Invoice", "Invoice", new { Id = userId });
            }

            return View("Guarantee_Invoice", guarantee);
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
            } while (_context.guarantee.Any(b => b.guarantee_Code == code));

            return code;
        }
    }
}
