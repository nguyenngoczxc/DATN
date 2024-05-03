using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TTTN3.Models
{
    [Table("Tproduct")]
    public class product 
    {
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? product_Code { get; set; }
        public string? product_Name { get; set; } 
        [ForeignKey("brand")]
        public string? brand_Code { get; set; }

        public string? product_Avatar { get; set; }
        [ForeignKey("material")]

        public string? material_Code { get; set; }
        [ForeignKey("zipper")]

        public string? zipper_Code { get; set; }
        [ForeignKey("wheel")]

        public string? wheel_Code { get; set; }
        //[Required]
        public int? quantity { get; set; }
        public int? weight { get; set; }
        
        public bool sale { get; set; }
        public string? description { get; set; }
        public virtual brand? brand { get; set; }
        public virtual zipper? zipper { get; set; }
        public virtual wheel? wheel { get; set; }

        public virtual material? material { get; set; }
        public virtual ICollection<product_Image> product_Images { get; set; } = new List<product_Image>();
        public virtual ICollection<invoice_Detail> invoice_Details { get; set; } = new List<invoice_Detail>();
        public virtual ICollection<rating> ratings { get; set; } = new List<rating>();
        public virtual ICollection<product_Size> product_Sizes { get; set; } = new List<product_Size>();
        public virtual ICollection<product_Color>? product_Colors { get; set; }
        public virtual ICollection<FavoriteProduct> FavoriteProduct { get; set; } = new List<FavoriteProduct>();
    }
}