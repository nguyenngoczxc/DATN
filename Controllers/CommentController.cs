using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTTN3.Models;

namespace TTTN3.Controllers
{
    [AllowAnonymous]
    public class CommentController : Controller
    {
        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signinManager;
        public CommentController(DataContext _db, UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signinManager, IWebHostEnvironment webHostEnvironment)
        {
            db = _db;
            _userManager = userManager;
            _signinManager = signinManager;
            _webHostEnvironment = webHostEnvironment;
        }
        //public IActionResult Index()
        //{
        //    var comments= db.comments.Include(c => c.AspNetUser).Include(c => c.product).ToList();
        //    return View();
        //}
        //public IActionResult Create()
        //{
        //    // Action để hiển thị form tạo comment
        //    return View();
        //}

        [HttpPost]
        public async Task<IActionResult> Create_Comment(string ProductCode, string CommentText)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                ViewBag.UserAvatar = user != null ? user.avatar : "default-avatar.png";
                ViewBag.UserId = user != null ? user.Id : null;
                // Lưu ý: Bạn cần xác thực người dùng ở đây, có thể sử dụng User.Identity hoặc các cách xác thực khác
                if (ModelState.IsValid)
                {
                    try
                    {
                        var comment = new comment
                        {
                            comment_Date = DateTime.Now,
                            comment_Code = GenerateUniqueCode(),
                            product_Code = ProductCode,
                            content = CommentText,
                            AspNetUserId = user.Id,
                        };
                        db.comments.Add(comment);
                        db.SaveChanges();
                        return RedirectToAction("Product_Details", "Shop", new { product_Code = ProductCode });
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error: " + ex.Message);
                    }
                }
            }
            else
            {
                // Người dùng không được xác thực, xử lý trường hợp này
                // Ví dụ: chuyển hướng người dùng đến trang đăng nhập
                return RedirectToAction("Login", "Account");
            }
            // Nếu ModelState không hợp lệ, hiển thị lại form với thông báo lỗi
            return View();
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
            } while (db.comments.Any(b => b.comment_Code == code));

            return code;
        }

        [HttpPost]
        public IActionResult Edit_Comment(string comment_Code, string content)
        {
            // Tìm comment bằng comment code
            var comment = db.comments.FirstOrDefault(c => c.comment_Code == comment_Code);
            if (comment == null)
            {
                return NotFound();
            }

            // Cập nhật nội dung của comment
            comment.content = content;
            db.SaveChanges();

            // Trả về một JSON response chỉ ra thành công
            return Json(new { success = true });
        }
       
        [HttpPost]
        public IActionResult Reply_Comment(string productCode, string parentCommentId, string replyText)
        {
            if (ModelState.IsValid)
            {
                // Lưu ý: Trong thực tế, bạn cần xác thực người dùng và kiểm tra quyền trước khi lưu comment vào cơ sở dữ liệu

                try
                {
                    // Tạo mới một comment reply
                    comment replyComment = new comment
                    {
                        comment_Code = GenerateUniqueCode(),
                        product_Code = productCode, // Mã sản phẩm của comment
                        parentComment_Id = parentCommentId, // ID của comment mà đang được reply
                        content = replyText, // Nội dung của comment reply
                        AspNetUserId = _userManager.GetUserId(User), // ID của người dùng hiện tại
                        comment_Date = DateTime.Now // Ngày tạo comment
                    };

                    // Lưu comment reply vào cơ sở dữ liệu
                    db.comments.Add(replyComment);
                    db.SaveChanges();

                    // Redirect tới trang chi tiết sản phẩm sau khi thêm comment thành công
                    return RedirectToAction("Product_Details", "Shop", new { product_Code = productCode });
                }
                catch (Exception ex)
                {
                    // Xử lý nếu có lỗi xảy ra trong quá trình lưu comment
                    ModelState.AddModelError("", "An error occurred while replying to the comment.");
                    // Nếu có lỗi, hiển thị lại form với thông báo lỗi
                    return View();
                }
            }

            // Nếu ModelState không hợp lệ, hiển thị lại form với thông báo lỗi
            return View();
        }
        [HttpPost]
        public IActionResult Remove_Comment(string comment_Code)
        {
            try
            {
                // Find the comment by comment code
                var comment = db.comments.FirstOrDefault(c => c.comment_Code == comment_Code);

                if (comment == null)
                {
                    return NotFound(); // Comment not found
                }

                // Check if the user is authorized to remove the comment
                if (User.Identity.IsAuthenticated && comment.AspNetUserId == _userManager.GetUserId(User))
                {
                    // Remove the comment from the database
                    db.comments.Remove(comment);
                    db.SaveChanges();

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Unauthorized to remove this comment." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

    }
}
