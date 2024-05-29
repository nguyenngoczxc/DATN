using System;
using System.Collections.Generic;
using System.Globalization;
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
    [Route("admin/guarantee")]
    [Authorize("Admin")]

    public class guaranteeController : Controller
    {
        private readonly DataContext _context;

        public guaranteeController(DataContext context)
        {
            _context = context;
        }

        // GET: Admin/guarantees
        [Route("List_Guarantee")]
        public async Task<IActionResult> List_Guarantee(string SearchText, int? page)
        {
            CultureInfo.CurrentCulture = new CultureInfo("vi-VN");
            const int pageSize = 10; // Number of items per page

            IQueryable<guarantee> ds = _context.guarantee
                .Include(p=>p.invoice_Detail)
                .ThenInclude(a=>a.invoice)
                .OrderByDescending(x => x.guarantee_Date);

            var pageNumber = page ?? 1; // If no page is specified, default to page 1
            if (!string.IsNullOrEmpty(SearchText))
            {
                if (SearchText.Equals("đã nhận", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Filter for incomplete guarantees (status == 1)
                    ds = ds.Where(x => x.status == 1);
                }
                else if (SearchText.Equals("xử lý", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Filter for incomplete guarantees (status == 1)
                    ds = ds.Where(x => x.status == 2);
                }
                else if (SearchText.Equals("hoàn thành", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Filter for incomplete guarantees (status == 1)
                    ds = ds.Where(x => x.status == 3);
                }
                else
                {
                    // Normal search based on guarantee_Code, CustomerName, or guarantee_Date
                    ds = ds.Where(x => x.guarantee_Code.Equals(SearchText)
                                       || x.customer.Contains(SearchText)
                                       || x.product.Contains(SearchText));

                }
            }
            //var pagedguarantees = ds.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            PagedList<guarantee> pagedguarantees = new PagedList<guarantee>(ds, pageNumber, pageSize);
            ViewData["SearchText"] = SearchText;
            ViewData["DbContext"] = _context;
            return View(pagedguarantees);
        }
        [Route("Complete_guarantee")]
        [HttpGet]
        public IActionResult Complete_guarantee(string guarantee_Code)
        {
            if (string.IsNullOrEmpty(guarantee_Code))
            {
                return NotFound();
            }

            var guarantee = _context.guarantee.FirstOrDefault(g => g.guarantee_Code == guarantee_Code);
            if (guarantee == null)
            {
                return NotFound();
            }

            return View(guarantee);
        }
        [HttpGet]
        [Route("ViewGuarantee")]
        public IActionResult ViewGuarantee(string guarantee_Code)
        {
            if (string.IsNullOrEmpty(guarantee_Code))
            {
                return NotFound();
            }

            var guarantee = _context.guarantee.FirstOrDefault(g => g.guarantee_Code == guarantee_Code);
            if (guarantee == null)
            {
                return NotFound();
            }

            return View(guarantee);
        }
        [Route("Complete_guarantee")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Complete_guarantee(guarantee guarantee)
        {
            if (ModelState.IsValid)
            {
                guarantee.status = 3;
                _context.Update(guarantee);
                _context.SaveChanges();
                return RedirectToAction("List_guarantee", "guarantee");
            }

            return View(guarantee);
        }
        [Route("DeleteGuarantee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteGuarantee(string guarantee_Code)
        {
            if (string.IsNullOrEmpty(guarantee_Code))
            {
                return NotFound();
            }

            var guarantee = _context.guarantee.FirstOrDefault(g => g.guarantee_Code == guarantee_Code);
            if (guarantee == null)
            {
                return NotFound();
            }

            _context.guarantee.Remove(guarantee);
            _context.SaveChanges();
            return RedirectToAction("List_guarantee", "guarantee");

        }
        [Route("Solution_guarantee")]
        public async Task<IActionResult> Solution_guarantee(string guarantee_Code)
        {
            if (guarantee_Code == null)
            {
                return NotFound();
            }

            var guarantee = await _context.guarantee.FindAsync(guarantee_Code);
            if (guarantee == null)
            {
                return NotFound();
            }
            guarantee.status = 2;

            // Lưu các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Redirect hoặc trả về một view tuỳ thuộc vào yêu cầu của bạn


            return RedirectToAction("List_guarantee", "guarantee");
        }
    }
}
