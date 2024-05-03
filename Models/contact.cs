using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTTN3.Models
{
    [Table("Tcontact")]
    public class contact
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? contact_Code {  get; set; }
        public string? contact_Name {  get; set; }
        public string? message {  get; set; }
        public string? email {  get; set; }
    }
}
