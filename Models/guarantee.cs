using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TTTN3.Models
{
    [Table("Tguarantee")]
    public partial class guarantee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? guarantee_Code { get; set; }
        [ForeignKey("invoice_Detail")]
        public string? invoice_Detail_Code { get; set; }
        public string? customer { get; set; }
        public string? product { get; set; }
        public DateTime? guarantee_Date { get; set; }
        public int status { get; set; }
        public string? guarantee_Description { get; set; }
        public string? guarantee_Solution { get; set; }
        public virtual invoice_Detail? invoice_Detail { get; set; }

    }
}
