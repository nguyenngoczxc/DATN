using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
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
    [Route("admin/contacts")]
    [Authorize("Admin")]

    public class contactsController : Controller
    {
        private readonly DataContext _context;

        public contactsController(DataContext context)
        {
            _context = context;
        }

        // GET: Admin/contacts
        [Route("List_Contact")]
        public async Task<IActionResult> List_Contact(string SearchText, int? page)
        {
            int pageSize = 10;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            var lstSanPham = _context.contacts.AsNoTracking().OrderBy(x => x.contact_Name).ToList();
            if (!string.IsNullOrEmpty(SearchText))
            {
                lstSanPham = lstSanPham.Where(x => x.contact_Code.Equals(SearchText) || x.contact_Name.Contains(SearchText)).ToList();
            }
            ViewData["SearchText"] = SearchText;

            //var lst = lstSanPham.ToPagedList(lstSanPham,pageNumber, pageSize);
            PagedList<contact> lst = new PagedList<contact>(lstSanPham, pageNumber, pageSize);
            return View(lst);
        }
        [Route("Details_Contact")]
        [HttpGet]
        // GET: Admin/contacts/Details/5
        public async Task<IActionResult> Details_Contact(string contact_Code)
        {
            if (contact_Code == null)
            {
                return NotFound();
            }

            var contact = await _context.contacts
                .FirstOrDefaultAsync(m => m.contact_Code == contact_Code);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }
        [Route("Create_Contact")]
        [HttpGet]
        // GET: Admin/contacts/Create
        public IActionResult Create_Contact()
        {
            return View();
        }

        // POST: Admin/contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("Create_Contact")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create_Contact([Bind("contact_Code,contact_Name,message,email")] contact contact)
        {
            if (ModelState.IsValid)
            {
               contact.contact_Code = GenerateUniqueCode();
                _context.Add(contact);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(List_Contact));
            }
            return View(contact);
        }

        // GET: Admin/contacts/Edit/5
        [Route("Edit_Contact")]
        [HttpGet]
        public async Task<IActionResult> Edit_Contact(string contact_Code)
        {
            if (contact_Code == null)
            {
                return NotFound();
            }

            var contact = await _context.contacts.FindAsync(contact_Code);
            if (contact == null)
            {
                return NotFound();
            }
            return View(contact);
        }

        // POST: Admin/contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("Edit_Contact")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit_Contact(string contact_Code, [Bind("contact_Code,contact_Name,message,email")] contact contact)
        {
            if (contact_Code != contact.contact_Code)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contact);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!contactExists(contact.contact_Code))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("List_Contact", "contacts");
            }
            return View(contact);
        }
        [Route("Delete_Contact")]
        [HttpGet]
        // GET: Admin/contacts/Delete/5
        public async Task<IActionResult> Delete_Contact(string contact_Code)
        {
            var contact = await _context.contacts
        .SingleOrDefaultAsync(m => m.contact_Code == contact_Code);

            ViewBag.ContactCode = contact_Code; // Pass contact code to the view

            return View(contact);
        }

        // POST: Admin/contacts/Delete/5
        [Route("Delete_Contact")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete_ContactConfirmed(string contact_Code)
        {
            try
            {
                var contact = await _context.contacts.FindAsync(contact_Code);

                   _context.Remove(contact);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(List_Contact));
               
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
        private bool contactExists(string contact_Code)
        {
            return _context.contacts.Any(e => e.contact_Code == contact_Code);
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
            } while (_context.contacts.Any(b => b.contact_Code == code));

            return code;
        }
    }
}
