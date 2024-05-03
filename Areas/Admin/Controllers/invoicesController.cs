using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TTTN3.Models;
using X.PagedList;

namespace TTTN3.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin")]
    [Route("admin/invoices")]
    [Authorize("Admin")]

    public class invoicesController : Controller
    {
        private readonly DataContext _context;

        public invoicesController(DataContext context)
        {
            _context = context;
        }

        // GET: Admin/invoices
        [Route("List_Invoice")]
        public async Task<IActionResult> List_Invoice(string SearchText, int? page)
        {
            const int pageSize = 10; // Number of items per page

            IQueryable<invoice> ds = _context.invoices
                .OrderByDescending(x => x.invoice_Date);

            var pageNumber = page ?? 1; // If no page is specified, default to page 1
            if (!string.IsNullOrEmpty(SearchText))
            {
                if (SearchText.Equals("COD", StringComparison.OrdinalIgnoreCase))
                {
                    // Filter for incomplete invoices (status == 1)
                    ds = ds.Where(x => x.type_Payment == 1);
                }
                else if (SearchText.Equals("Transfer", StringComparison.OrdinalIgnoreCase))
                {
                    // Filter for incomplete invoices (status == 1)
                    ds = ds.Where(x => x.type_Payment == 2);
                }
                else if (SearchText.Equals("order", StringComparison.OrdinalIgnoreCase))
                {
                    // Filter for incomplete invoices (status == 1)
                    ds = ds.Where(x => x.status == 1);
                }
                else if (SearchText.Equals("transport", StringComparison.OrdinalIgnoreCase))
                {
                    // Filter for incomplete invoices (status == 1)
                    ds = ds.Where(x => x.status == 2);
                }
                else if (SearchText.Equals("complete", StringComparison.OrdinalIgnoreCase))
                {
                    // Filter for incomplete invoices (status == 1)
                    ds = ds.Where(x => x.status == 3);
                }
                else if (SearchText.Equals("delete", StringComparison.OrdinalIgnoreCase))
                {
                    // Filter for incomplete invoices (status == 1)
                    ds = ds.Where(x => x.status == 4);
                }
                else
                {
                    // Normal search based on invoice_Code, CustomerName, or invoice_Date
                    ds = ds.Where(x => x.invoice_Code.Equals(SearchText)
                                       || x.CustomerName.Contains(SearchText)
                                       || x.invoice_Date.Equals(SearchText));
                }
            }
            //var pagedInvoices = ds.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            PagedList<invoice> pagedInvoices = new PagedList<invoice>(ds, pageNumber, pageSize);
            ViewData["SearchText"] = SearchText;

            return View(pagedInvoices);
        }

        // GET: Admin/invoices/Details/5
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
                .Where(m => m.invoice_Code == invoice_Code).ToListAsync();
            if (invoice == null)
            {
                return NotFound();
            }
            var infor = await _context.invoices.FirstOrDefaultAsync(i => i.invoice_Code == invoice_Code);

            ViewBag.UserName = infor.CustomerName;
            ViewBag.Phone = infor.Phone;
            ViewBag.Address = infor.Address;
            ViewBag.Email = infor.Email;
            return View(invoice);
        }

        //public async Task UpdateInvoiceStatus()
        //{
        //    var threeDaysAgo = DateTime.Now.AddDays(-3);
        //    var invoicesToUpdate = _context.invoices.Where(i => i.status != 2 && i.invoice_Date <= threeDaysAgo).ToList();

        //    foreach (var invoice in invoicesToUpdate)
        //    {
        //        invoice.status = 2;
        //    }

        //    await _context.SaveChangesAsync(); // Lưu tất cả các thay đổi vào cơ sở dữ liệu chỉ một lần
        //}
        public async Task UpdateInvoiceStatus()
        {
            var today = DateTime.Now;
            var oneDayAgo = today.AddDays(-1);
            var threeDaysAgo = today.AddDays(-3);

            // Cập nhật trạng thái thành 2 sau một ngày
            var invoicesToUpdateStatus2 = _context.invoices
                .Where(i => i.status == 1 && i.invoice_Date <= oneDayAgo)
                .ToList();

            foreach (var invoice in invoicesToUpdateStatus2)
            {
                invoice.status = 2;
            }

            // Cập nhật trạng thái thành 3 sau ba ngày
            var invoicesToUpdateStatus3 = _context.invoices
                .Where(a => a.status == 2 && a.invoice_Date <= threeDaysAgo)
                .ToList();

            foreach (var invoice in invoicesToUpdateStatus3)
            {
                invoice.status = 3;
            }

            await _context.SaveChangesAsync(); // Lưu tất cả các thay đổi vào cơ sở dữ liệu chỉ một lần
        }
        
        [Route("Complete_Invoice")]
        public async Task<IActionResult> Complete_Invoice(string invoice_Code)
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
            // Đặt trạng thái của đơn hàng thành "complete"
            invoice.status = 3;

            // Lưu các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Redirect hoặc trả về một view tuỳ thuộc vào yêu cầu của bạn


            return RedirectToAction("List_Invoice", "invoices");
        }
        [Route("Transport_Invoice")]
        public async Task<IActionResult> Transport_Invoice(string invoice_Code)
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
            // Đặt trạng thái của đơn hàng thành "vận chuyển"
            invoice.status = 2;

            // Lưu các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Redirect hoặc trả về một view tuỳ thuộc vào yêu cầu của bạn


            return RedirectToAction("List_Invoice", "invoices");
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


            return RedirectToAction("List_Invoice", "invoices");
        }

        //[Route("Delete_Invoice")]
        //public async Task<IActionResult> Delete_Invoice(string invoice_Code)
        //{
        //    if (invoice_Code == null)
        //    {
        //        return NotFound();
        //    }

        //    var invoice = await _context.invoices.FindAsync(invoice_Code);
        //    if (invoice == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(invoice);
        //}

        //[HttpPost]
        //[Route("Delete_Invoice")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed_Invoice(string invoice_Code)
        //{

        //    var invoice = await _context.invoices.FindAsync(invoice_Code);
        //    if (invoice == null)
        //    {
        //        return NotFound();
        //    }
        //    var detail_invoices = _context.invoice_Details.Where(x => x.invoice_Code == invoice_Code);
        //    if (detail_invoices.Any())
        //        _context.RemoveRange(detail_invoices);
        //    // Delete the invoice
        //    _context.invoices.Remove(invoice);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction("List_Invoice", "invoices", new { area = "admin" });
        //}

    }
}
