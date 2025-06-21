// Data/Models/Product.cs
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace GoodMorningFactory.Data.Models
{
    // --- بداية الإضافة: تعريف أنواع التتبع ---
    public enum ProductTrackingMethod
    {
        [Description("لا يوجد تتبع")]
        None,
        [Description("حسب الرقم التسلسلي")]
        BySerialNumber,
        [Description("حسب رقم الدفعة")]
        ByLotNumber
    }
    // --- نهاية الإضافة ---

    public enum ProductType
    {
        [Description("منتج نهائي")]
        FinishedGood,
        [Description("مادة خام")]
        RawMaterial,
        [Description("منتج نصف مصنع")]
        WorkInProgress,
        [Description("خدمة")]
        Service
    }

    public class Product
    {
        // --- معلومات أساسية ---
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProductCode { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Barcode { get; set; }

        [Required]
        public ProductType ProductType { get; set; }

        // --- بداية الإضافة: خاصية طريقة التتبع ---
        [Required]
        public ProductTrackingMethod TrackingMethod { get; set; } = ProductTrackingMethod.None;
        // --- نهاية الإضافة ---

        public bool IsActive { get; set; } = true;

        // --- التصنيف والوحدة ---
        [Required]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        public int? UnitOfMeasureId { get; set; }
        public virtual UnitOfMeasure UnitOfMeasure { get; set; }

        // --- التسعير والتكلفة ---
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PurchasePrice { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SalePrice { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal AverageCost { get; set; }

        [Required]
        public int CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        public int? TaxRuleId { get; set; }
        public virtual TaxRule TaxRule { get; set; }

        // --- معلومات إضافية ---
        public byte[]? ProductImage { get; set; }

        public int? DefaultSupplierId { get; set; }
        public virtual Supplier DefaultSupplier { get; set; }

        public int LeadTimeDays { get; set; }

        // --- العلاقات ---
        public virtual ICollection<PurchaseRequisitionItem> PurchaseRequisitionItems { get; set; } = new List<PurchaseRequisitionItem>();
    }
}
