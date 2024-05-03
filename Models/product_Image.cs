using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTTN3.Models
{
    [Table("Tproduct_Image")]
    public partial class product_Image
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? image_Code { get; set; } 
        [ForeignKey("product")]
        public string? product_Code { get; set; }

        public string? image_Filename { get; set; } 


        public virtual product? product { get; set; }
    }
}
