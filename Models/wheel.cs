using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TTTN3.Models
{
    [Table("Twheel")]
    public partial class wheel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? wheel_Code { get; set; }
        [Required]
        public string? wheel_Name { get; set; }
        public virtual ICollection<product> products { get; set; } = new List<product>();
    }
}
