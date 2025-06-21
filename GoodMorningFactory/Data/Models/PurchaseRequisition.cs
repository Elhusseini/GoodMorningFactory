// Data/Models/PurchaseRequisition.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum RequisitionStatus
    {
        [Description("مسودة")]
        Draft,
        [Description("قيد الموافقة")]
        PendingApproval,
        [Description("موافق عليه")]
        Approved,
        [Description("مرفوض")]
        Rejected,
        [Description("تم إنشاء أمر شراء")]
        PO_Created,
        [Description("ملغي")]
        Cancelled
    }

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
        [MaxLength(100)]
        public string RequesterName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Department { get; set; }

        public string? Purpose { get; set; }

        [Required]
        public RequisitionStatus Status { get; set; }

        // --- بداية الإضافة: حقل لربط الطلب بنظام الموافقات ---
        public int? ApprovalRequestId { get; set; }
        public virtual ApprovalRequest ApprovalRequest { get; set; }
        // --- نهاية الإضافة ---

        public virtual ICollection<PurchaseRequisitionItem> PurchaseRequisitionItems { get; set; }
    }
}
