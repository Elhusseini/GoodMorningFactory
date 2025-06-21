// Data/Models/ApprovalRequest.cs
// ملف جديد: يمثل طلب موافقة فعلي على مستند معين
using GoodMorningFactory.Data.Models;
using System; // تأكد من وجود هذا الاستخدام لـ DateTime
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    // --- بداية الإضافة: خاصية ApprovalWorkflowId ---
    public int? ApprovalWorkflowId { get; set; }
    public virtual ApprovalWorkflow ApprovalWorkflow { get; set; }
    // --- نهاية الإضافة ---

    // --- بداية الإضافة: خاصية CurrentApproverRoleId ---
    public int? CurrentApproverRoleId { get; set; }
    public virtual Role CurrentApproverRole { get; set; }
    // --- نهاية الإضافة ---

    [Required]
    public ApprovalStatus Status { get; set; }

    public string RejectionReason { get; set; }

    public DateTime RequestDate { get; set; } // كان اسمها RequestedDate في الخطأ، لكنها هنا RequestDate
    public DateTime? LastActionDate { get; set; }
}