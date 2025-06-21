// Data/Models/Product.cs
// *** تحديث: تم إصلاح حقل وحدة القياس ليكون مفتاحاً أجنبياً ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum ProductType { RawMaterial, FinishedGood }

    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProductCode { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public ProductType ProductType { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        // --- بداية الإصلاح: تحويل وحدة القياس إلى علاقة ---
        public int? UnitOfMeasureId { get; set; }
        public virtual UnitOfMeasure UnitOfMeasure { get; set; }
        // --- نهاية الإصلاح ---

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PurchasePrice { get; set; } // سعر التكلفة/الشراء

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SalePrice { get; set; } // سعر البيع

        public byte[]? ProductImage { get; set; } // حقل لتخزين صورة المنتج

        public bool IsActive { get; set; } = true;
    }
}