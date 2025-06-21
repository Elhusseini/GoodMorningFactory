using System.Collections.Generic;
using System.Collections.ObjectModel; // <-- تأكد من وجود هذا السطر
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
        public DocumentType DocumentType { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MinimumAmount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // *** التعديل الأساسي: استخدام ObservableCollection لتحديث الواجهة تلقائياً ***
        public virtual ICollection<ApprovalWorkflowStep> Steps { get; set; } = new ObservableCollection<ApprovalWorkflowStep>();
    }
}
