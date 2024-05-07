using Microsoft.AspNetCore.Mvc;
using TTTN3.Models;
using TTTN3.Models.ViewModels;
using TTTN3.Responsitory;

namespace TTTN3.ViewComponents
{
    public class CheckOut : ViewComponent
    {
        private readonly DataContext db;
        public CheckOut(DataContext dba)
        {
            db = dba;

        }
       
        public IViewComponentResult Invoke()
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            var viewModel = new CartItemViewModel
            {
                CartItems = cartItems,
                GrandTotal = CalculateGrandTotal(cartItems)
            };

            return View("Partial_Item", viewModel);
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
    }
}
