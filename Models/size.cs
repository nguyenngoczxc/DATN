using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TTTN3.Models
{
        [Table("Tsize")]
        public partial class size
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public string? size_Code { get; set; }

            public string? size_Name { get; set; }

        public virtual ICollection<product_Size> product_Sizes { get; set; } = new List<product_Size>();
        
        }
    
}
