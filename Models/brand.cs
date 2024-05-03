using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TTTN3.Models
{
    [Table("Tbrand")]
    public partial class brand
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? brand_Code { get; set; }
        [Required]
        public string? brand_Name { get; set; } 
        public virtual ICollection<product> products { get; set; } = new List<product>();
    }
}
