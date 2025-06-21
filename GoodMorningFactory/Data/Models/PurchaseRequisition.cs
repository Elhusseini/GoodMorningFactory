// Data/Models/PurchaseRequisition.cs
// *** الكود الكامل لنموذج طلبات الشراء مع إضافة الحالة المفقودة ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    // *** بداية الإصلاح: إضافة الحالة المفقودة ***
    public enum RequisitionStatus { Draft, PendingApproval, Approved, Rejected, PO_Created, Cancelled }
    // *** نهاية الإصلاح ***

    public class PurchaseRequisition
    {
        public PurchaseRequisition()
        {
            PurchaseRequisitionItems = new HashSet<PurchaseRequisitionItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequisitionNumber { get; set; }

        [Required]
        public DateTime RequisitionDate { get; set; }

        [Required]
        public string RequesterName { get; set; }

        [Required]
        public string Department { get; set; }

        public string? Purpose { get; set; }

        [Required]
        public RequisitionStatus Status { get; set; }

        public virtual ICollection<PurchaseRequisitionItem> PurchaseRequisitionItems { get; set; }
    }
}