// GoodMorning/Data/Models/StockMovement.cs
// *** ملف جديد: يمثل سجل حركة مخزون مركزي واحد ***
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    // أنواع الحركات المخزنية
    public enum StockMovementType
    {
        [Description("استلام مشتريات")]
        PurchaseReceipt,
        [Description("صرف مبيعات")]
        SalesShipment,
        [Description("صرف للإنتاج")]
        ProductionConsumption,
        [Description("إدخال إنتاج تام")]
        FinishedGoodsProduction,
        [Description("مرتجع مبيعات")]
        SalesReturn,
        [Description("مرتجع مشتريات")]
        PurchaseReturn,
        [Description("تسوية جرد (زيادة)")]
        AdjustmentIncrease,
        [Description("تسوية جرد (نقص)")]
        AdjustmentDecrease,
        [Description("تحويل (خروج)")]
        TransferOut,
        [Description("تحويل (دخول)")]
        TransferIn
    }

    [Table("StockMovements")]
    public class StockMovement
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        public int StorageLocationId { get; set; }
        public virtual StorageLocation StorageLocation { get; set; }

        [Required]
        public DateTime MovementDate { get; set; }

        [Required]
        public StockMovementType MovementType { get; set; }

        [Required]
        public int Quantity { get; set; } // الكمية دائما موجبة، النوع يحدد التأثير

        [Column(TypeName = "decimal(18, 4)")]
        public decimal UnitCost { get; set; } // تكلفة الوحدة في وقت الحركة

        public string ReferenceDocument { get; set; } // رقم المستند المرجعي

        public int? UserId { get; set; }
        public virtual User User { get; set; }
    }
}
