using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTTN3.Models
{
    [Table("Tinvoice_Detail")]
    public partial class invoice_Detail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? invoice_Detail_Code { get; set; }
        [ForeignKey("invoice")]
        public string? invoice_Code { get; set; }
        [ForeignKey("product")]
        public string? product_Code { get; set; }
        [ForeignKey("size")]
        public string? size_Code { get; set; }
        [ForeignKey("color")]
        public string? color_Code { get; set; }
        public int? quantity_Sold { get; set; }

        public decimal? price { get; set; }

        public virtual invoice? invoice { get; set; }
        public virtual product? product { get; set; }
        public virtual size? size { get; set; }
        public virtual color? color { get; set; }
        public virtual ICollection<guarantee>? guarantee { get; set; }


    }

}
