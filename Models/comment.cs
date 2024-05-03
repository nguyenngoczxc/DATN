using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Web;

namespace TTTN3.Models
{
    [Table("Tcomment")]
    public class comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? comment_Code { get; set; }
        [ForeignKey("product")]
        public string? product_Code { get; set; }
        public string? content { get; set; }
        public string? parentComment_Id { get; set; }
        public DateTime? comment_Date { get; set; }
        public virtual product? product { get; set; }
        // Foreign key for the AspNetUser
        public string? AspNetUserId { get; set; }
        // Navigation property for the AspNetUser
        [ForeignKey("AspNetUserId")]
        public virtual AppUserModel AspNetUser { get; set; }


        public virtual comment? ParentComment { get; set; }
        public virtual ICollection<comment> comments { get; set; } = new List<comment>();



    }
}
