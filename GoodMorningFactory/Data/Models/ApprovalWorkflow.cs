// Data/Models/ApprovalWorkflow.cs
// ملف جديد: يمثل رأس دورة الموافقة (مثال: "موافقة طلبات الشراء فوق 5000")
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    [Table("ApprovalWorkflows")]
    public class ApprovalWorkflow
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [Required]
        public DocumentType DocumentType { get; set; } // نوع المستند الذي تنطبق عليه (مثل: طلب شراء)

        // شرط التفعيل (مثال: القيمة الدنيا لتفعيل الدورة)
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MinimumAmount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<ApprovalWorkflowStep> Steps { get; set; } = new List<ApprovalWorkflowStep>();
    }

    // Data/Models/ApprovalWorkflowStep.cs
    // ملف جديد: يمثل خطوة واحدة داخل دورة الموافقة
    [Table("ApprovalWorkflowSteps")]
    public class ApprovalWorkflowStep
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ApprovalWorkflowId { get; set; }
        public virtual ApprovalWorkflow ApprovalWorkflow { get; set; }

        [Required]
        public int StepOrder { get; set; } // ترتيب الخطوة (1, 2, 3...)

        [Required]
        [MaxLength(100)]
        public string StepName { get; set; } // اسم الخطوة (مثال: "موافقة المدير المباشر")

        [Required]
        public int ApproverRoleId { get; set; } // الدور الوظيفي المسؤول عن الموافقة
        public virtual Role ApproverRole { get; set; }
    }

    // Data/Models/ApprovalRequest.cs
    // ملف جديد: يمثل طلب موافقة فعلي على مستند معين
    public enum ApprovalStatus { Pending, Approved, Rejected, Cancelled }

    [Table("ApprovalRequests")]
    public class ApprovalRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DocumentId { get; set; } // هوية المستند (مثال: PurchaseRequisition.Id)

        [Required]
        public DocumentType DocumentType { get; set; }

        public int? CurrentStepId { get; set; }
        public virtual ApprovalWorkflowStep CurrentStep { get; set; }

        [Required]
        public ApprovalStatus Status { get; set; }

        public string RejectionReason { get; set; }

        public DateTime RequestDate { get; set; }
        public DateTime? LastActionDate { get; set; }
    }
}
