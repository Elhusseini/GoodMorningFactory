// Data/Models/Opportunity.cs
// ملف جديد: يمثل فرصة بيعية
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum OpportunityStage
    {
        [Description("تأهيل")]
        Qualification,
        [Description("تحليل الاحتياجات")]
        NeedsAnalysis,
        [Description("عرض السعر")]
        Proposal,
        [Description("تفاوض")]
        Negotiation,
        [Description("مغلقة (مربوحة)")]
        ClosedWon,
        [Description("مغلقة (خاسرة)")]
        ClosedLost
    }

    [Table("Opportunities")]
    public class Opportunity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        public int CustomerId { get; set; } // يرتبط بعميل فعلي
        public virtual Customer Customer { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal EstimatedValue { get; set; }

        public DateTime? CloseDate { get; set; }

        [Required]
        public OpportunityStage Stage { get; set; } = OpportunityStage.Qualification;

        public int? AssignedToUserId { get; set; }
        public virtual User AssignedToUser { get; set; }
    }
}
