using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TTTN3.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Cannot be left blank")]
        public string user_Name { get; set; }
        [DataType(DataType.Password), Required(ErrorMessage = "Cannot be left blank")]
        public string password { get; set; }
        public string? phone { get; set; }

        public string? address { get; set; }
        public string? avatar { get; set; }
        [Required(ErrorMessage = "Cannot be left blank"), EmailAddress]
        public string email { get; set; }
    }
}
