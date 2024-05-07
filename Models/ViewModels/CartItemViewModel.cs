using System.Collections.Generic;
using System.Linq;

namespace TTTN3.Models.ViewModels
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; }
        public decimal GrandTotal { get; set; }
        public List<SizeInfo> AllSizes { get; set; }
        public List<ColorInfo> AllColors { get; set; }

        public CartItemViewModel()
        {
            CartItems = new List<CartItemModel>();
            AllSizes = new List<SizeInfo>();
            AllColors = new List<ColorInfo>();
        }

        public CartItemViewModel(List<CartItemModel> cartItems, List<SizeInfo> allSizes, List<ColorInfo> allColors)
        {
            CartItems = cartItems;
            AllSizes = allSizes;
            AllColors = allColors;

            // Calculate grand total
            GrandTotal = CalculateGrandTotal();
        }

        private decimal CalculateGrandTotal()
        {
            decimal total = 0;
            foreach (var item in CartItems)
            {
                total += item.Sizes.Sum(size => size.Total);
            }
            return total;
        }

    }
}
