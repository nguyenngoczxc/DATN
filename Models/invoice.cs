using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTTN3.Models
{

        [Table("Tinvoice")]
    public partial class invoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string invoice_Code { get; set; }
        [Required(ErrorMessage = "This field cannot be left blank")]

        public string CustomerName { get; set; }
        [Required(ErrorMessage = "This field cannot be left blank")]

        public string Phone { get; set; }
        [Required(ErrorMessage = "This field cannot be left blank")]

        public string Address { get; set; }
        [EmailAddress(ErrorMessage = "Email address is not valid")]
        public string Email { get; set; }
        public DateTime? invoice_Date { get; set; }


        public decimal? total_Payment { get; set; }

        public string? CreateBy { get;set; }
        public int? type_Payment { get; set; }
        public int status { get; set; }
        public int total_Quantity { get;set; }

        public string? Note { get; set; }
        // Foreign key for the AspNetUser
        public string? customerId { get; set; }
        // Navigation property for the AspNetUser
       
        public virtual ICollection<invoice_Detail> invoice_Details { get; set; } = new List<invoice_Detail>();
    }

}
