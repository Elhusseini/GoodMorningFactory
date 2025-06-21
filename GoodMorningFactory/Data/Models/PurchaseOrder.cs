// Data/Models/PurchaseOrder.cs
// *** تحديث: تمت إضافة حالتي الاستلام والفوترة لتتبع أفضل للأوامر ***
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum PurchaseOrderStatus
    {
        [Description("مسودة")]
        Draft,
        [Description("مرسل")]
        Sent,
        [Description("مؤكد")]
        Confirmed,
        [Description("مستلم جزئياً")]
        PartiallyReceived,
        [Description("مستلم بالكامل")]
        FullyReceived,
        [Description("مفوتر")]
        Invoiced,
        [Description("ملغي")]
        Cancelled
    }

    // --- بداية الإضافة: تعريف الحالات الجديدة ---
    public enum ReceiptStatus
    {
        [Description("لم يتم الاستلام")]
        NotReceived,
        [Description("مستلم جزئياً")]
        PartiallyReceived,
        [Description("مستلم بالكامل")]
        FullyReceived
    }

    public enum POInvoicingStatus
    {
        [Description("غير مفوتر")]
        NotInvoiced,
        [Description("مفوتر جزئياً")]
        PartiallyInvoiced,
        [Description("مفوتر بالكامل")]
        FullyInvoiced
    }
    // --- نهاية الإضافة ---

    public class PurchaseOrder
    {
        public PurchaseOrder()
        {
            PurchaseOrderItems = new HashSet<PurchaseOrderItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string PurchaseOrderNumber { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        [Required]
        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public PurchaseOrderStatus Status { get; set; }

        // --- بداية الإضافة: إضافة الحقول الجديدة للجدول ---
        [Required]
        public ReceiptStatus ReceiptStatus { get; set; } = ReceiptStatus.NotReceived;

        [Required]
        public POInvoicingStatus InvoicingStatus { get; set; } = POInvoicingStatus.NotInvoiced;
        // --- نهاية الإضافة ---

        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    }
}
