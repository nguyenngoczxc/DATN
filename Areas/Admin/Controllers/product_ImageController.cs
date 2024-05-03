using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TTTN3.Models;
using X.PagedList;

namespace TTTN3.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin")]
    [Route("admin/product_Image")]
    [Authorize("Admin")]

    public class product_ImageController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public product_ImageController(DataContext context, IWebHostEnvironment webHostEnvironment, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Admin/product_Image
        [Route("List_Image")]
        [HttpGet]
        public async Task<IActionResult> List_Image(int? page)
        {
            const int pageSize = 10; // Number of items per page

            var dataContext = _context.product_Images
         .Include(p => p.product)
         .GroupBy(pi => pi.product_Code)
         .Select(group => group.First());
            var pageNumber = page ?? 1; // If no page is specified, default to page 1
            var pagedImages = await dataContext.ToPagedListAsync(pageNumber, pageSize);

            return View(pagedImages);
        }


        // GET: Admin/product_Image/Details/5
        [Route("Detail_Image")]
        [HttpGet]
        public async Task<IActionResult> Detail_Image(string product_Code)
        {
            if (product_Code == null)
            {
                return NotFound();
            }

            var productImages = await _context.product_Images
                .Where(pi => pi.product_Code == product_Code)
                .ToListAsync();

            if (productImages == null || productImages.Count == 0)
            {
                return NotFound();
            }
            ViewData["ProductCode"] = _context.products.Find(product_Code).product_Code;
            ViewData["ProductName"] = _context.products.Find(product_Code).product_Name;
            // Pass a list of product images to the view
            return View(productImages);
        }




        // GET: Admin/product_Image/Create
        [Route("Create_Image")]
        [HttpGet]
        public IActionResult Create_Image()
        {
            ViewData["product_Code"] = new SelectList(_context.products, "product_Code", "product_Name");
            return View();
        }

        [HttpPost]
        [Route("Create_Image")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create_Image(List<IFormFile> images, [Bind("product_Code")] product_Image product_Image)
        {
            product_Image.image_Code = GenerateUniqueCode();
            if (images != null && images.Count > 0)
            {
                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        // Save the file to the server or perform other necessary operations
                        // For simplicity, let's assume you save the file to the wwwroot/images folder
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Product", image.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        // Create a product_Image entity for each uploaded image
                        var productImage = new product_Image
                        {
                            product_Code = product_Image.product_Code,
                            image_Filename = image.FileName
                            // Add other properties as needed
                        };

                        // Add the product_Image entity to the context and save changes
                        _context.Add(productImage);
                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(List_Image));
            }

            ViewData["product_Code"] = new SelectList(_context.products, "product_Code", "product_Name", product_Image.product_Code);
            return View(product_Image);
        }

        private string GenerateUniqueCode()
        {
            Random random = new Random();
            string code;

            do
            {
                int randomCode = random.Next(1000, 10000);
                code = randomCode.ToString("D4"); // Ensure it has 4 digits
            } while (_context.product_Images.Any(b => b.image_Code == code));

            return code;
        }


        // GET: Admin/product_Image/Edit/5
        [HttpGet]
        [Route("Edit_Image")]
        public async Task<IActionResult> Edit_Image(string product_Code)
        {
            if (product_Code == null)
            {
                return NotFound();
            }

            var productImages = await _context.product_Images
                .Where(pi => pi.product_Code == product_Code)
                .ToListAsync();

            if (productImages == null || productImages.Count == 0)
            {
                return NotFound();
            }
            ViewData["ProductName"] = _context.products.Find(product_Code).product_Name;
            ViewData["ProductCode"] = _context.products.Find(product_Code).product_Code;
            // Pass a list of existing product_Images to the view
            return View(productImages);
        }


        [HttpPost]
        [Route("Edit_Image")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit_Image(
    List<IFormFile> newImages,
    [FromForm(Name = "existingImageFilenames[]")] List<string> existingImageFilenames,
    string product_Code)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //product_Code = ViewData["ProductCode"].ToString();
                    ViewData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý.";
                    // 1. Xác định danh sách ảnh mới từ form gửi lên
                    var newImageFilenames = new List<string>();
                    if (newImages != null && newImages.Count > 0)
                    {
                        foreach (var newImage in newImages)
                        {
                            if (newImage.Length > 0)
                            {
                                // Lưu ảnh mới lên server
                                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Product", newImage.FileName);
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await newImage.CopyToAsync(stream);
                                }

                                // Thêm tên file mới vào danh sách
                                newImageFilenames.Add(newImage.FileName);
                            }
                        }
                    }

                    // 2. Xóa bất kỳ ảnh nào không còn trong danh sách mới
                    var imagesToRemove = existingImageFilenames.Except(newImageFilenames).ToList();
                    foreach (var imageToRemove in imagesToRemove)
                    {
                        // Xóa ảnh từ server nếu cần
                        var filePathToRemove = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Product", imageToRemove);
                        if (System.IO.File.Exists(filePathToRemove))
                        {
                            System.IO.File.Delete(filePathToRemove);
                        }

                        // Xóa ảnh khỏi danh sách của sản phẩm
                        var imageToRemoveEntity = await _context.product_Images.FirstOrDefaultAsync(pi => pi.product_Code == product_Code && pi.image_Filename == imageToRemove);
                        if (imageToRemoveEntity != null)
                        {
                            _context.product_Images.Remove(imageToRemoveEntity);
                        }
                    }

                    // 3. Thêm ảnh mới vào danh sách của sản phẩm
                    foreach (var newImageFilename in newImageFilenames)
                    {
                        _context.product_Images.Add(new product_Image
                        {
                            product_Code = product_Code,
                            image_Filename = newImageFilename
                            // Các thuộc tính khác của product_Image nếu có
                        });
                    }

                    // Lưu thay đổi vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(List_Image));
                }
                catch (Exception ex)
                {
                    ViewData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý.";
                    // Xử lý lỗi nếu cần
                    return View(existingImageFilenames);
                }
            }

            return View(existingImageFilenames);
        }


        // GET: Admin/product_Image/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete_Image(string product_Code)
        {
            if (product_Code == null)
            {
                return NotFound();
            }

            var productImages = await _context.product_Images
                .Where(pi => pi.product_Code == product_Code)
                .ToListAsync();

            if (productImages == null || productImages.Count == 0)
            {
                return NotFound();
            }
            ViewData["ProductCode"] = _context.products.Find(product_Code).product_Code;
            ViewData["ProductName"] = _context.products.Find(product_Code).product_Name;
            // Pass a list of product images to the view
            return View(productImages);
        }

        [HttpPost, ActionName("Delete_Image")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed_Image(string product_Code)
        {
            // Retrieve the product images related to the specified product_Code
            var productImages = await _context.product_Images
                .Where(pi => pi.product_Code == product_Code)
                .ToListAsync();

            if (productImages != null && productImages.Count > 0)
            {
                // Loop through each product image
                foreach (var productImage in productImages)
                {
                    // Delete logic for each file, e.g., deleting from storage
                    // For simplicity, let's assume you delete from the wwwroot/images folder
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images/Product", productImage.image_Filename);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    // Remove the product image entity from the context
                    _context.product_Images.Remove(productImage);
                }

                // Save changes to the database
                await _context.SaveChangesAsync();
            }

            // Redirect to the list view or any other desired action
            return RedirectToAction(nameof(List_Image));
        }

        private bool product_ImageExists(string image_Code)
        {
            return _context.product_Images.Any(e => e.image_Code == image_Code);
        }
        private bool ProductExists(string product_Code)
        {
            return _context.products.Any(e => e.product_Code == product_Code);
        }
    }
}
