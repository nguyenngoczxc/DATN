using Microsoft.AspNetCore.Identity;
namespace TTTN3.Models
{
    public class AppUserModel : IdentityUser
    {
        public string? avatar { get; set; }
        public string? address { get; set; }
        public string? phone { get; set; }
        public virtual ICollection<FavoriteProduct> FavoriteProduct { get; set; }
        public virtual ICollection<guarantee> guarantee { get; set; }
    }
}
