// Data/Models/PurchaseOrder.cs
// *** الكود الكامل لنموذج أمر الشراء مع إضافة جميع الحالات ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    // تم إضافة جميع الحالات المخطط لها
    public enum PurchaseOrderStatus { Draft, Sent, Confirmed, PartiallyReceived, FullyReceived, Invoiced, Cancelled }

    public class PurchaseOrder
    {
        // تهيئة قائمة البنود لضمان أنها ليست فارغة
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

        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    }
}
