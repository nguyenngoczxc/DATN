using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTTN3.Models
{
    [Table("Tblog")]
    public class blog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? blog_Code { get; set; }
        public string? blog_Title { get;set; }
        public string? alias {  get; set; }
        public string? blog_Detail {  get; set; }
        public string? blog_Image { get; set; }
        public DateTime? blog_Date { get; set; }
    }
}
