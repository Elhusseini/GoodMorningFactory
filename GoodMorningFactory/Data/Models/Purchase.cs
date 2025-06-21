// Data/Models/Purchase.cs
// *** الكود الكامل لنموذج فاتورة المشتريات مع إضافة حقل لربطها بمذكرة الاستلام ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    // تعريف حالات فاتورة المورد
    public enum PurchaseInvoiceStatus { Draft, ApprovedForPayment, PartiallyPaid, FullyPaid, Cancelled }

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
        public string InvoiceNumber { get; set; } // رقم فاتورة المورد

        [Required]
        public DateTime PurchaseDate { get; set; } // تاريخ فاتورة المورد

        public DateTime? DueDate { get; set; } // تاريخ الاستحقاق

        [Required]
        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        public int? PurchaseOrderId { get; set; } // اختياري، لربطه بأمر الشراء
        public virtual PurchaseOrder PurchaseOrder { get; set; }

        // --- بداية التحديث: إضافة حقل جديد لربطه بمذكرة الاستلام ---
        public int? GoodsReceiptNoteId { get; set; }
        public virtual GoodsReceiptNote GoodsReceiptNote { get; set; }
        // --- نهاية التحديث ---

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