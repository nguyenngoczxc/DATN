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
using Microsoft.AspNetCore.Identity;

namespace TTTN3.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin")]
    [Route("admin/comments")]
    [Authorize("Admin")]

    public class commentsController : Controller
    {
        private readonly DataContext _context;
        private UserManager<AppUserModel> _userManager;

        public commentsController(DataContext context)
        {
            _context = context;
        }

        // GET: Admin/comments
        [Route("List_Comment")]
        //public async Task<IActionResult> List_Comment(string SearchText, int? page)
        //{
        //    IQueryable<comment> dataContext = _context.comments.Include(c => c.AspNetUser).Include(c => c.product);

        //    if (!string.IsNullOrEmpty(SearchText))
        //    {
        //        dataContext = dataContext.Where(x => x.comment_Code.Equals(SearchText) || x.product.product_Name.Contains(SearchText) || x.AspNetUser.UserName.Contains(SearchText));
        //    }

        //    int pageSize = 10;
        //    int pageNumber = page == null || page < 1 ? 1 : page.Value;

        //    IQueryable<comment> lst = dataContext.Select(c => new comment
        //    {
        //        // Map properties accordingly, assuming contact has similar properties as comment
        //        comment_Code = c.comment_Code,
        //        // Other properties...
        //    });
        //    ViewData["SearchText"] = SearchText;

        //    PagedList<comment> pagedData = new PagedList<comment>(lst, pageNumber, pageSize);

        //    return View(pagedData);
        //}
        public async Task<IActionResult> List_Comment(string SearchText, int? page)
        {
            IQueryable<comment> dataContext = _context.comments.Include(c => c.AspNetUser).Include(c => c.product);

            if (!string.IsNullOrEmpty(SearchText))
            {
                dataContext = dataContext
                    .Where(x => x.comment_Code.Equals(SearchText) ||
                                x.product.product_Name.Contains(SearchText) ||
                                x.AspNetUser.UserName.Contains(SearchText));
            }

            int pageSize = 10;
            int pageNumber = page == null || page < 1 ? 1 : page.Value;

            PagedList<comment> pagedData = new PagedList<comment>(dataContext, pageNumber, pageSize);

            ViewData["SearchText"] = SearchText;

            return View(pagedData);
        }


        // GET: Admin/comments/Details/5
        [Route("Details_Comment")]
        public async Task<IActionResult> Details_Comment(string comment_Code)
        {
            if (comment_Code == null)
            {
                return NotFound();
            }

            var comment = await _context.comments
                .Include(c => c.AspNetUser)
                .Include(c => c.product)
                .FirstOrDefaultAsync(m => m.comment_Code == comment_Code);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Admin/comments/Create
        [Route("Create_Comment ")]
        [HttpGet]
        public IActionResult Create_Comment()
        {
            ViewData["customer_Code"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["product_Code"] = new SelectList(_context.products, "product_Code", "product_Name");
            return View();
        }

        // POST: Admin/comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.System
        [HttpPost]
        [Route("Create_Comment ")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create_Comment([Bind("comment_Code,product_Code,customer_Code,content")] comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.comment_Code = GenerateUniqueCode();
                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(List_Comment));
            }

            ViewData["customer_Code"] = new SelectList(_context.Users, "Id", "UserName", comment.AspNetUserId);
            ViewData["product_Code"] = new SelectList(_context.products, "product_Code", "product_Name", comment.product_Code);

            // You may need additional logic to populate dropdown for selecting the parent comment
            ViewData["parentComment_Id"] = new SelectList(_context.comments, "comment_Code", "content");

            return View(comment);
        }

        // GET: Admin/comments/Edit/5
        [Route("Edit_Comment ")]
        [HttpGet]

        public async Task<IActionResult> Edit_Comment(string comment_Code)
        {
            var commentToEdit = await _context.comments.FindAsync(comment_Code);

            if (commentToEdit == null)
            {
                // If the comment is not found, return a not found response or redirect to a suitable action
                return NotFound();
            }

            // Populate ViewData for dropdowns if needed
            ViewData["customer_Code"] = new SelectList(_context.Users, "Id", "UserName", commentToEdit.AspNetUserId);
            ViewData["product_Code"] = new SelectList(_context.products, "product_Code", "product_Name", commentToEdit.product_Code);

            // You may need additional logic to populate dropdown for selecting the parent comment
            ViewData["parentComment_Id"] = new SelectList(_context.comments, "comment_Code", "content");

            return View(commentToEdit);
        }

        // POST: Admin/comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit_Comment ")]

        public async Task<IActionResult> Edit_Comment(string comment_Code, [Bind("comment_Code,product_Code,customer_Code,content,parentComment_Id")] comment comment)
        {
            if (comment_Code != comment.comment_Code)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the comment in the database
                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!commentExists(comment.comment_Code))
                    {
                        // If the comment is not found, return a not found response or redirect to a suitable action
                        return NotFound();
                    }
                    else
                    {
                        // Handle other concurrency issues if needed
                        throw;
                    }
                }

                // Redirect to the list of comments after successful update
                return RedirectToAction(nameof(List_Comment));
            }

            // Populate ViewData for dropdowns if needed
            ViewData["customer_Code"] = new SelectList(_context.Users, "Id", "UserName", comment.AspNetUserId);
            ViewData["product_Code"] = new SelectList(_context.products, "product_Code", "product_Name", comment.product_Code);

            // You may need additional logic to populate dropdown for selecting the parent comment
            ViewData["parentComment_Id"] = new SelectList(_context.comments, "comment_Code", "content");

          
            return View(comment);
        }

        // GET: Admin/comments/Delete/5
        [Route("Delete_Comment")]
        [HttpGet]
        public async Task<IActionResult> Delete_Comment(string comment_Code)
        {
            if (comment_Code == null)
            {
                return NotFound();
            }

            var comments = await _context.comments
                .Include(c => c.AspNetUser)
                .Include(c => c.product)
                .FirstOrDefaultAsync(m => m.comment_Code == comment_Code);
            if (comments == null)
            {
                return NotFound();
            }

            return View(comments);
        }

        // POST: Admin/comments/Delete/5
        [HttpPost, ActionName("Delete_Comment")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Delete_Comment_Confirmed(string comment_Code)
        {
            var comments = await _context.comments.FindAsync(comment_Code);
            if (comments != null)
            {
                _context.comments.Remove(comments);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List_Comment));
        }
       
        private bool commentExists(string comment)
        {
            return _context.comments.Any(e => e.comment_Code == comment);
        }
        
        private string GenerateUniqueCode()
        {
            Random random = new Random();
            string code;

            do
            {
                int randomCode = random.Next(1000, 10000);
                code = randomCode.ToString("D4"); // Ensure it has 4 digits
            } while (_context.comments.Any(b => b.comment_Code == code));

            return code;
        }
    }
}
