using X.PagedList;

namespace TTTN3.Models
{
    public class ProductListViewModel
    {
        public List<product> Products { get; set; }
        public List<material> Materials { get; set; }
        public Dictionary<string, decimal> MinPrices { get; set; }
    }
}
