using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTTN3.Models;
using X.PagedList;

namespace TTTN3.Controllers
{
    [Route("Invoice")]
    public class InvoiceController : Controller
    {
        private readonly DataContext _context;

        public InvoiceController(DataContext context)
        {
            _context = context;
        }
        [Route("List_Invoice")]
        public async Task<IActionResult> List_Invoice(string SearchText, int? page,string Id)
        {
            const int pageSize = 10; // Number of items per page

            IQueryable<invoice> ds = _context.invoices.Where(m => m.customerId == Id)
                .OrderByDescending(x => x.invoice_Date);

            var pageNumber = page ?? 1; // If no page is specified, default to page 1
            if (!string.IsNullOrEmpty(SearchText))
            {
                ds = ds.Where(x => x.invoice_Code.Equals(SearchText) || x.invoice_Date.Equals(SearchText));
            }
            //var pagedInvoices = ds.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            PagedList<invoice> pagedInvoices = new PagedList<invoice>(ds, pageNumber, pageSize);
            ViewData["SearchText"] = SearchText;

            return View(pagedInvoices);
        }
        [Route("Details_Invoice")]

        public async Task<IActionResult> Details_Invoice(string invoice_Code)
        {
            if (invoice_Code == null)
            {
                return NotFound();
            }

            var invoice = await _context.invoice_Details
                .Include(x => x.color)
                .Include(x => x.product)
                .Include(x => x.size)
                .Include(x => x.invoice)
                .Include(d => d.guarantee)
                .Where(m => m.invoice_Code == invoice_Code).ToListAsync();
            if (invoice == null)
            {
                return NotFound();
            }
            var infor = await _context.invoices.FirstOrDefaultAsync(i => i.invoice_Code == invoice_Code);
            
            ViewBag.UserName = infor.CustomerName;
            ViewBag.Phone = infor.Phone;
            ViewBag.Address = infor.Address;
            ViewBag.Status = infor.status;
            ViewBag.Email = infor.Email;
            return View(invoice);
        }

        [Route("Delete_Invoice")]
        public async Task<IActionResult> Delete_Invoice(string invoice_Code)
        {
            if (invoice_Code == null)
            {
                return NotFound();
            }

            var invoice = await _context.invoices.FindAsync(invoice_Code);
            if (invoice == null)
            {
                return NotFound();
            }

            // Lặp qua mỗi sản phẩm trong đơn hàng và cập nhật số lượng hoàn lại
            var invoiceDetails = _context.invoice_Details.Where(d => d.invoice_Code == invoice_Code).ToList();
            foreach (var item in invoiceDetails)
            {
                var product = await _context.products.FindAsync(item.product_Code);
                if (product != null)
                {
                    // Tăng số lượng sản phẩm trong kho khi hủy đơn hàng
                    product.quantity += item.quantity_Sold;
                }
            }

            // Đặt trạng thái của đơn hàng thành "hủy"
            invoice.status = 4;

            // Lưu các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Redirect hoặc trả về một view tuỳ thuộc vào yêu cầu của bạn
            return RedirectToAction("Index", "Home");
        }

    }
}
