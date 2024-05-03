using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TTTN3.Models
{
    [Table("Tzipper")]
    public partial class zipper
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? zipper_Code { get; set; }
        [Required]
        public string? zipper_Name { get; set; }
        public virtual ICollection<product> products { get; set; } = new List<product>();
    }
}
