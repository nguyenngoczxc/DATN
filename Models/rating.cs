using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTTN3.Models
{
    [Table("Trating")]
    public class rating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? rating_Code {  get; set; }
        [ForeignKey("product")]
        public string? product_Code { get;set; }
        public int Rate {  get; set; }

        public virtual product? product { get; set; }
        // Foreign key for the AspNetUser
        public string AspNetUserId { get; set; }
        // Navigation property for the AspNetUser
        [ForeignKey("AspNetUserId")]
        public AppUserModel AspNetUser { get; set; }
    }
}
