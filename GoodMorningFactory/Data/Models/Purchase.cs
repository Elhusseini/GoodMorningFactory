// Data/Models/Purchase.cs
// *** الكود الكامل والمعدل ***
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
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

    public class Purchase
    {
        public Purchase()
        {
            PurchaseItems = new HashSet<PurchaseItem>();
            PurchaseReturns = new HashSet<PurchaseReturn>();
            GoodsReceiptNotes = new HashSet<GoodsReceiptNote>();
        }

        [Key]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string InvoiceNumber { get; set; }
        [Required]
        public DateTime PurchaseDate { get; set; }
        public DateTime? DueDate { get; set; }
        [Required]
        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }
        public int? PurchaseOrderId { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; }
        [Required, Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountPaid { get; set; }
        [Required]
        public PurchaseInvoiceStatus Status { get; set; }

        // ======================= بداية الإضافة =======================
        /// <summary>
        /// حالة الإرجاع للفاتورة.
        /// </summary>
        [Required]
        public PurchaseReturnStatus ReturnStatus { get; set; } = PurchaseReturnStatus.NotReturned;
        // ======================== نهاية الإضافة ========================

        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; }
        public virtual ICollection<PurchaseReturn> PurchaseReturns { get; set; }
        public virtual ICollection<GoodsReceiptNote> GoodsReceiptNotes { get; set; }
    }
}