using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTTN3.Models
{
    [Table("Tadmin")]

    public partial class admin
    {
        [Key]
        public string? username { get; set; } = null!;
        [Required]

        public string? password { get; set; } 
        public string? avatar { get; set; } 

    }


}
