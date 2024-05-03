using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TTTN3.Models
{
    [Table("Tmaterial")]
   
    public partial class material
    {
       
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? material_Code { get; set; }
        public string? material_Name { get; set; } 

        public virtual ICollection<product> products { get; set; } = new List<product>();
    }
}
