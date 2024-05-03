using System.ComponentModel.DataAnnotations.Schema;

namespace TTTN3.Models
{
    public class FavoriteProduct
    {
        public string Id { get; set; }
        [ForeignKey("UserId")]
        public string UserId { get; set; }

        public AppUserModel User { get; set; }

        [ForeignKey("ProductId")]
        public string ProductId { get; set; }

        public product? product { get; set; }
    }
}
