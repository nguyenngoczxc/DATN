using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TTTN3.Models
{
        [Table("Tproduct_Color")]
        public class product_Color
        {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? product_Color_Code { get; set; }

        [ForeignKey("product")]
        public string? product_Code { get; set; }

        [ForeignKey("color")]
        public string? color_Code { get; set; }

        public virtual product? product { get; set; }

        public virtual color? color { get; set; }
    }
    
}
