using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace TTTN3.Models
{
    [Table("Tproduct_Size")]
    public class product_Size
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? product_Size_Code { get; set; }

        [ForeignKey("product")]
        public string? product_Code { get; set; }

        [ForeignKey("size")]
        public string? size_Code { get; set; }

        public decimal Price { get; set; }

        public virtual product? product { get; set; }

        public virtual size? size { get; set; }
    }
}
