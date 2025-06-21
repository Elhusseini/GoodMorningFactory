// Data/Models/Purchase.cs
// *** تحديث: تمت إضافة وصف عربي لحالات الفاتورة ***
using System;
using System.Collections.Generic;
using System.ComponentModel; // <-- إضافة مهمة
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    // --- بداية التحديث: إضافة وصف عربي للحالات ---
    public enum PurchaseInvoiceStatus
    {
        [Description("مسودة")]
        Draft,
        [Description("موافق عليها للدفع")]
        ApprovedForPayment,
        [Description("مدفوعة جزئياً")]
        PartiallyPaid,
        [Description("مدفوعة بالكامل")]
        FullyPaid,
        [Description("ملغاة")]
        Cancelled
    }
    // --- نهاية التحديث ---

    public class Purchase
    {
        public Purchase()
        {
            PurchaseItems = new HashSet<PurchaseItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string InvoiceNumber { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; }

        public DateTime? DueDate { get; set; }

        [Required]
        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        public int? PurchaseOrderId { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; }

        public int? GoodsReceiptNoteId { get; set; }
        public virtual GoodsReceiptNote GoodsReceiptNote { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountPaid { get; set; }

        [Required]
        public PurchaseInvoiceStatus Status { get; set; }

        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; }
    }
}
