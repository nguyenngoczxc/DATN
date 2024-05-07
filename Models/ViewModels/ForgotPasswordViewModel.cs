using System.ComponentModel.DataAnnotations;

namespace TTTN3.Models.ViewModels
{
  // ForgotPasswordViewModel.cs
        public class ForgotPasswordViewModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            public bool EmailSent { get; set; }
        }
    
}
