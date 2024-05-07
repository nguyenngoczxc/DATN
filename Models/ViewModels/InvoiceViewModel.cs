using System.ComponentModel.DataAnnotations;

namespace TTTN3.Models.ViewModels
{
    public class InvoiceViewModel
    {
        [Required(ErrorMessage = "This field cannot be left blank")]

        public string CustomerName { get; set; }
        [Required(ErrorMessage = "This field cannot be left blank")]

        public string Phone { get; set; }
        [EmailAddress(ErrorMessage = "Email address is not valid")]
        public string Email { get; set; }
        [Required(ErrorMessage = "This field cannot be left blank")]
        public string Address { get; set; }
        public string? CustomerId { get; set; }
        public int? TypePayment { get; set; }
        public string? Note { get; set; }

    }
}
