using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TTTN3.Models;
using TTTN3.Models.ViewModels;
using TTTN3.Responsitory;

namespace TTTN3.Controllers
{
    [Route("Cart")]
    //[Authorize]
    public class CartController : Controller
    {
        private readonly DataContext db;

        public CartController(DataContext _db)
        {
            db = _db;
        }
        
        [Route("Index")]
        public IActionResult Index()
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            var viewModel = new CartItemViewModel
            {
                CartItems = cartItems,
                AllSizes = GetProductSizes(),
                AllColors = GetProductColors(),
                GrandTotal = CalculateGrandTotal(cartItems)
            };

            return View(viewModel);
        }

        // Phương thức để lấy thông tin về kích thước sản phẩm từ cơ sở dữ liệu
        private List<SizeInfo> GetProductSizes()
        {
            var allSizes = db.sizes.ToList();
            if (allSizes != null)
                // Chuyển đổi danh sách kích thước từ cơ sở dữ liệu sang danh sách SizeInfo
                return allSizes.Select(size => new SizeInfo
                {
                    Size_Code = size.size_Code,
                    Size_Name = size.size_Name,
                    //Price=size.product_Sizes.FirstOrDefault(x=>x.size_Code==size.size_Code).Price
                }).ToList();
            return new List<SizeInfo>();
        }

        // Phương thức để lấy thông tin về màu sắc sản phẩm từ cơ sở dữ liệu
        private List<ColorInfo> GetProductColors()
        {
            var allColors = db.colors.ToList();
            if (allColors != null)
                // Chuyển đổi danh sách màu sắc từ cơ sở dữ liệu sang danh sách ColorInfo
                return allColors.Select(color => new ColorInfo { Color_Code = color.color_Code, Color_Name = color.color_Name }).ToList();
            return new List<ColorInfo>();
        }

        private decimal CalculateGrandTotal(List<CartItemModel> cartItems)
        {
            decimal total = 0;
            foreach (var item in cartItems)
            {
                total += item.Sizes.Sum(size => size.Total);
            }
            return total;
        }

        private List<SizeInfo> GetUniqueProductSizes(List<CartItemModel> cartItems)
        {
            // Lấy tất cả các kích thước độc nhất từ tất cả các sản phẩm trong giỏ hàng
            List<SizeInfo> allSizes = new List<SizeInfo>();
            foreach (var item in cartItems)
            {
                allSizes.AddRange(item.Sizes);
            }
            return allSizes.GroupBy(s => s.Size_Code)
                           .Select(g => g.First())
                           .ToList();
        }

        private List<ColorInfo> GetUniqueProductColors(List<CartItemModel> cartItems)
        {
            // Lấy tất cả các màu sắc độc nhất từ tất cả các sản phẩm trong giỏ hàng
            List<ColorInfo> allColors = new List<ColorInfo>();
            foreach (var item in cartItems)
            {
                allColors.AddRange(item.Colors);
            }
            return allColors.GroupBy(c => c.Color_Code)
                            .Select(g => g.First())
                            .ToList();
        }


        [Route("Add")]
        [HttpPost]
        public IActionResult Add(string product_Code, string selectedSizeCode, int quantity, decimal price, string color_Code)
        {
            var product = db.products.Find(product_Code);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // Tạo một đối tượng CartItemModel mới
            var cartItem = new CartItemModel
            {
                Product_Code = product.product_Code,
                Product_Name = product.product_Name,
                Product_Avatar = product.product_Avatar,
                Sizes = new List<SizeInfo> { new SizeInfo { Quantity = quantity, Size_Code = selectedSizeCode, Price = price, Selected = true, Size_Name = GetSizeName(selectedSizeCode) } },
                Colors = new List<ColorInfo> { new ColorInfo { Color_Code = color_Code, Color_Name = GetColorName(color_Code) } }
            };

            // Lấy giỏ hàng từ session
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            // Kiểm tra xem sản phẩm đã tồn tại trong giỏ hàng chưa
            var existingCartItem = cartItems.FirstOrDefault(item =>
                item.Product_Code == product_Code &&
                item.Colors.Any(color => color.Color_Code == color_Code) &&
                item.Sizes.Any(size => size.Size_Code == selectedSizeCode));

            if (existingCartItem != null)
            {
                // Nếu sản phẩm đã tồn tại trong giỏ hàng, chỉ cập nhật số lượng cho kích thước đã chọn
                var existingSize = existingCartItem.Sizes.FirstOrDefault(size => size.Size_Code == selectedSizeCode);
                if (existingSize != null)
                {
                    existingSize.Quantity += quantity;
                }
                else
                {
                    // Nếu kích thước không tồn tại trong sản phẩm đã có, thêm kích thước mới với số lượng đã chọn
                    existingCartItem.Sizes.Add(new SizeInfo
                    {
                        Size_Code = selectedSizeCode,
                        Size_Name = GetSizeName(selectedSizeCode),
                        Price = price,
                        Quantity = quantity,
                        Selected = true
                    });
                }
            }
            else
            {
                // Nếu sản phẩm chưa tồn tại trong giỏ hàng, thêm sản phẩm vào giỏ hàng
                cartItems.Add(cartItem);
            }

            // Lưu giỏ hàng vào session
            HttpContext.Session.SetJson("Cart", cartItems);

            // Chuyển hướng trở lại trang trước đó
            // return Redirect(Request.Headers["Referer"].ToString());
            return Redirect("Index");
        }
        private string GetSizeName(string sizeCode)
        {
            var size = db.sizes.FirstOrDefault(s => s.size_Code == sizeCode);
            return size != null ? size.size_Name : "";
        }

        // Phương thức để lấy tên màu sắc từ mã màu sắc
        private string GetColorName(string colorCode)
        {
            var color = db.colors.FirstOrDefault(c => c.color_Code == colorCode);
            return color != null ? color.color_Name : "";
        }
        [Route("UpdateCart")]
        [HttpPost]
        public IActionResult UpdateCart(string color, string size, string product_Code, string selectedSizeCode, int quantity, string selectedColorCode)
        {
            
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            // Kiểm tra xem có sản phẩm nào trong giỏ hàng thỏa mãn điều kiện hay không
            var existingItem = cartItems.FirstOrDefault(item => item.Product_Code == product_Code && item.Colors.Any(c => c.Color_Code == color) && item.Sizes.Any(s => s.Size_Code == size));

            if (existingItem != null)
            {
                // Xóa sản phẩm hiện tại khỏi giỏ hàng
                cartItems.Remove(existingItem);
            }

            var product = db.products
                .Include(p => p.product_Sizes)
                .FirstOrDefault(p => p.product_Code == product_Code);

            if (product == null)
            {
                return NotFound("Product not found.");
            }
            else
            {
                var price = product.product_Sizes.FirstOrDefault(x => x.size_Code == selectedSizeCode).Price;
                // Tạo một đối tượng CartItemModel mới
                var cartItem = new CartItemModel
                {
                    Product_Code = product.product_Code,
                    Product_Name = product.product_Name,
                    Product_Avatar = product.product_Avatar,
                    Sizes = new List<SizeInfo> { new SizeInfo { Quantity = quantity, Size_Code = selectedSizeCode, Price = price, Selected = true, Size_Name = GetSizeName(selectedSizeCode) } },
                    Colors = new List<ColorInfo> { new ColorInfo { Color_Code = selectedColorCode, Color_Name = GetColorName(selectedColorCode) } }
                };


                // Kiểm tra xem sản phẩm đã tồn tại trong giỏ hàng chưa
                var existingCartItem = cartItems.FirstOrDefault(item =>
                    item.Product_Code == product_Code &&
                    item.Colors.Any(color => color.Color_Code == selectedColorCode) &&
                    item.Sizes.Any(size => size.Size_Code == selectedSizeCode));

                if (existingCartItem != null)
                {
                    // Nếu sản phẩm đã tồn tại trong giỏ hàng, chỉ cập nhật số lượng cho kích thước đã chọn
                    var existingSize = existingCartItem.Sizes.FirstOrDefault(size => size.Size_Code == selectedSizeCode);
                    if (existingSize != null)
                    {
                        existingSize.Quantity = quantity;
                    }
                    else
                    {
                        // Nếu kích thước không tồn tại trong sản phẩm đã có, thêm kích thước mới với số lượng đã chọn
                        existingCartItem.Sizes.Add(new SizeInfo
                        {
                            Size_Code = selectedSizeCode,
                            Size_Name = GetSizeName(selectedSizeCode),
                            Price = price,
                            Quantity = quantity,
                            Selected = true
                        });
                    }
                }
                else
                {
                    // Nếu sản phẩm chưa tồn tại trong giỏ hàng, thêm sản phẩm vào giỏ hàng
                    cartItems.Add(cartItem);
                }

                // Lưu giỏ hàng vào session
                HttpContext.Session.SetJson("Cart", cartItems);

                // Chuyển hướng trở lại trang trước đó
                return RedirectToAction("Index");
            }
        }


        [Route("DeleteCart")]
        [HttpPost]
        public IActionResult DeleteCart(string color, string size, string product_Code)
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            // Kiểm tra xem có sản phẩm nào trong giỏ hàng thỏa mãn điều kiện hay không
            var existingItem = cartItems.FirstOrDefault(item => item.Product_Code == product_Code && item.Colors.Any(c => c.Color_Code == color) && item.Sizes.Any(s => s.Size_Code == size));

            if (existingItem != null)
            {
                // Xóa sản phẩm hiện tại khỏi giỏ hàng
                cartItems.Remove(existingItem);
            }
            // Lưu giỏ hàng vào session
            HttpContext.Session.SetJson("Cart", cartItems);

            // Chuyển hướng trở lại trang trước đó
            return RedirectToAction("Index");
        }
    }

}
