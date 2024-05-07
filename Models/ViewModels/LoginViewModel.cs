using System.ComponentModel.DataAnnotations;

namespace TTTN3.Models.ViewModels
{
    public class LoginViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Cannot be left blank")]
        public string user_Name { get; set; }
        [DataType(DataType.Password), Required(ErrorMessage = "Cannot be left blank")]
        public string password { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
