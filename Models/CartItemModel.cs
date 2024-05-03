using System.Collections.Generic;
using System.Linq;

namespace TTTN3.Models.ViewModels
{
    public class CartItemModel
    {
        public string Product_Code { get; set; }
        public string Product_Name { get; set; }
        public string Product_Avatar { get; set; }
        public List<SizeInfo> Sizes { get; set; }
        public List<ColorInfo> Colors { get; set; }
       
        public CartItemModel() { }

        public CartItemModel(product product)
        {
            Product_Code = product.product_Code;
            Product_Name = product.product_Name;
            Product_Avatar = product.product_Avatar;
            // Default quantity is 1

            Sizes = product.product_Sizes.Select(ps => new SizeInfo
            {
                Size_Code = ps.size.size_Code,
                Size_Name = ps.size.size_Name,
                Price = ps.Price,
                Quantity = 1
            }).ToList();

            Colors = product.product_Colors.Select(pc => new ColorInfo
            {
                Color_Code = pc.color.color_Code,
                Color_Name = pc.color.color_Name
            }).ToList();
        }

    }

    public class SizeInfo
    {
        public string Size_Code { get; set; }
        public string Size_Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool Selected { get; set; }
        public decimal Total { get { return Quantity * Price; } }

    }

    public class ColorInfo
    {
        public string Color_Code { get; set; }
        public string Color_Name { get; set; }
    }
}
