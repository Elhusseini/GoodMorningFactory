using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace GoodMorningFactory.Data.Models
{
    public enum ProductTrackingMethod
    {
        [Description("لا يوجد تتبع")]
        None,
        [Description("حسب الرقم التسلسلي")]
        BySerialNumber,
        [Description("حسب رقم الدفعة")]
        ByLotNumber
    }

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

        [Required]
        public ProductTrackingMethod TrackingMethod { get; set; } = ProductTrackingMethod.None;

        // --- بداية الإضافة ---
        [Required]
        public bool TrackInventory { get; set; } = true; // الخاصية التي كانت ناقصة
        // --- نهاية الإضافة ---

        public bool IsActive { get; set; } = true;

        [Required]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        public int? UnitOfMeasureId { get; set; }
        public virtual UnitOfMeasure UnitOfMeasure { get; set; }

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

        public byte[]? ProductImage { get; set; }

        public int? DefaultSupplierId { get; set; }
        public virtual Supplier DefaultSupplier { get; set; }

        public int LeadTimeDays { get; set; }

        public virtual ICollection<PurchaseRequisitionItem> PurchaseRequisitionItems { get; set; } = new List<PurchaseRequisitionItem>();
    }
}