using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using TTTN3.Models;
using X.PagedList;

namespace TTTN3.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin")]
    [Route("admin/Promotion")]
    [Authorize("Admin")]

    public class PromotionController : Controller
    {
        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PromotionController(IWebHostEnvironment webHostEnvironment, DataContext _db)
        {
            db = _db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Route("List_Promotion")]

        public IActionResult List_Promotion(string SearchText, int? page)
        {
            int pageSize = 5;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            IQueryable<promotion> lstSanPham = db.promotion.AsNoTracking();
            if (!string.IsNullOrEmpty(SearchText))
            {
                lstSanPham = lstSanPham.Where(x => x.promotion_Code.Equals(SearchText) || x.promotion_Title.Contains(SearchText));
            }
            ViewData["SearchText"] = SearchText;

            //  var lst = lstSanPham.ToPagedList(pageNumber, pageSize);
            PagedList<promotion> lst = new PagedList<promotion>(lstSanPham, pageNumber, pageSize);
            return View(lst);
        }
        [Route("Create_Promotion")]
        [HttpGet]
        public IActionResult Create_Promotion()
        {
            return View();
        }

        [Route("Create_Promotion")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create_Promotion(promotion Promotion, IFormFile anhDaiDienFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Promotion.promotion_Code = GenerateUniqueCode();
                    Promotion.select = false;
                    if (anhDaiDienFile != null && anhDaiDienFile.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(anhDaiDienFile.FileName);
                        string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Promotion");
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            anhDaiDienFile.CopyTo(stream);
                        }
                        Promotion.promotion_Image = uniqueFileName;
                    }
                    db.promotion.Add(Promotion);
                    db.SaveChanges();
                    return RedirectToAction("List_Promotion", "Promotion");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }
            return View(Promotion);
        }

        [Route("Edit_Promotion")]
        [HttpGet]
        public IActionResult Edit_Promotion(string Promotion_Code)
        {
            var Promotion = db.promotion.Find(Promotion_Code);
            return View(Promotion);
        }
        [Route("Edit_Promotion")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit_Promotion(promotion PromotionUp, IFormFile anhDaiDienFile)
        {
                try
                {
                    var existing= db.promotion.Find(PromotionUp.promotion_Code);
                    if (anhDaiDienFile != null && anhDaiDienFile.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(anhDaiDienFile.FileName);
                        string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Promotion");
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            anhDaiDienFile.CopyTo(stream);
                        }
                        existing.promotion_Image = uniqueFileName;
                    }
                else
                {
                    existing.promotion_Image = existing.promotion_Image;
                }
                    existing.promotion_Title = PromotionUp.promotion_Title;
                existing.promotion_Detail = PromotionUp.promotion_Detail;
                    db.SaveChanges();
                    return RedirectToAction("List_Promotion", "Promotion");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            
            return View(PromotionUp);
        }
        //[HttpGet]
        //////[ValidateAntiForgeryToken]
        //[Route("Delete_Promotion")]
        //public IActionResult Delete_Promotion(string Promotion_Code)
        //{
        //    TempData["Message"] = "";


        //        var Promotion = db.promotions.Find(Promotion_Code);

        //        if (Promotion != null)
        //        {
        //            db.Remove(Promotion);
        //            db.SaveChanges();
        //            TempData["Message"] = "Promotion is deleted";
        //        }
        //        else
        //        {
        //            TempData["Message"] = "Promotion not found";
        //        }


        //    return RedirectToAction("List_Promotion", "Promotion");
        //}
        [Route("Delete_Promotion")]
        [HttpGet]
        public IActionResult Delete_Promotion(string Promotion_Code)
        {
            var Promotion = db.promotion.SingleOrDefault(x => x.promotion_Code == Promotion_Code);
            ViewBag.promotionCode = Promotion_Code; // Pass brand code to the view
            return View(Promotion);
        }

        [Route("Delete_Promotion")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete_PromotionConfirmed(string Promotion_Code)
        {
            try
            {
                var Promotion = db.promotion.Find(Promotion_Code);
                db.Remove(Promotion);
                db.SaveChanges();
                return RedirectToAction("List_Promotion", "Promotion");

            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpGet]
        public IActionResult Promotion_Detail(string Promotion_Code)
        {
            var Promotion = db.promotion.SingleOrDefault(x => x.promotion_Code == Promotion_Code);
            return View(Promotion);
        }
        [HttpPost]
        [Route("UpdateSelect")]
        public JsonResult UpdateSelect(string promotionCode)
        {
            try
            {
                // Lấy khuyến mãi theo mã
                var promotion = db.promotion.Find(promotionCode);
                if (promotion != null)
                {
                    // Cập nhật trạng thái select
                    promotion.select = !promotion.select;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    db.SaveChanges();

                    // Trả về phản hồi thành công
                    return Json(new { success = true });
                }
                else
                {
                    // Trả về phản hồi thất bại nếu không tìm thấy khuyến mãi
                    return Json(new { success = false, message = "Khuyến mãi không tồn tại." });
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về phản hồi thất bại
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
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
            } while (db.promotion.Any(b => b.promotion_Code == code));

            return code;
        }
    }
}
