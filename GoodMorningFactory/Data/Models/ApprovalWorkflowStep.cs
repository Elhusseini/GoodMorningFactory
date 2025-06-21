using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodMorningFactory.Data.Models
{
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
}
