using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTTN3.Models
    {
        [Table("Tpromotion")]
        public class promotion
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public string? promotion_Code { get; set; }
            public string? promotion_Title { get; set; }
            public bool? select { get; set; }
            public string? promotion_Detail { get; set; }
            public string? promotion_Image { get; set; }
        }
    }
