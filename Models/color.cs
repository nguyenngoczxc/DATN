using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TTTN3.Models
{
    [Table("Tcolor")]
    public partial class color
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? color_Code { get; set; }

        public string? color_Name { get; set; }

        public virtual ICollection<product_Color> product_Colors { get; set; } = new List<product_Color>();

    }


}