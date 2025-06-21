// Data/Models/SaleItem.cs
// يمثل سطراً واحداً (بنداً) في فاتورة المبيعات.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class SaleItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SaleId { get; set; } // مفتاح أجنبي يربطه بفاتورة البيع
        public virtual Sale Sale { get; set; }

        [Required]
        public int ProductId { get; set; } // مفتاح أجنبي يربطه بالمنتج المباع
        public virtual Product Product { get; set; }

        [Required]
        public int Quantity { get; set; } // الكمية المباعة من هذا المنتج

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; } // سعر الوحدة عند البيع (قد يختلف عن السعر الافتراضي)
    }
}