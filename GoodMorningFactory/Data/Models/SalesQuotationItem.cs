// Data/Models/SalesQuotationItem.cs
// *** ملف جديد: يمثل سطراً واحداً (بنداً) في عرض السعر ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class SalesQuotationItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SalesQuotationId { get; set; }
        public virtual SalesQuotation SalesQuotation { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public string Description { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Discount { get; set; } // مبلغ الخصم
    }
}