// GoodMorningFactory/Data/Models/Opportunity.cs
using System;
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
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal EstimatedValue { get; set; }

        public DateTime? CloseDate { get; set; }

        [Required]
        public OpportunityStage Stage { get; set; } = OpportunityStage.Qualification;

        public int? AssignedToUserId { get; set; }
        public virtual User AssignedToUser { get; set; }

        // --- بداية الإضافة: حقل لربط الفرصة بعرض السعر ---
        /// <summary>
        /// يُستخدم لتخزين رقم عرض السعر (ID) الذي تم إنشاؤه من هذه الفرصة.
        /// قيمته تكون null إذا لم يتم إنشاء عرض سعر بعد.
        /// </summary>
        public int? GeneratedQuotationId { get; set; }
        // --- نهاية الإضافة ---
    }
}