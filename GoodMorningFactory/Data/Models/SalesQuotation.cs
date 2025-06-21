// GoodMorningFactory/Data/Models/SalesQuotation.cs
// *** تحديث: تمت إضافة الوصف العربي للحالات (enum) ***
using System;
using System.Collections.Generic;
using System.ComponentModel; // <-- إضافة مهمة لاستخدام [Description]
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    // --- بداية التعديل: إضافة الوصف العربي ---
    public enum QuotationStatus
    {
        [Description("مسودة")]
        Draft,

        [Description("تم الإرسال")]
        Sent,

        [Description("مقبول")]
        Accepted,

        [Description("مرفوض")]
        Rejected,

        [Description("منتهي الصلاحية")]
        Expired,

        [Description("مغلق")]
        Closed
    }
    // --- نهاية التعديل ---

    public class SalesQuotation
    {
        public SalesQuotation()
        {
            SalesQuotationItems = new HashSet<SalesQuotationItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string QuotationNumber { get; set; }

        [Required]
        public DateTime QuotationDate { get; set; }

        [Required]
        public DateTime ValidUntilDate { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Subtotal { get; set; } // الإجمالي قبل الضريبة

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TaxAmount { get; set; } // مبلغ الضريبة

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; } // الإجمالي النهائي بعد الضريبة

        [Required]
        public QuotationStatus Status { get; set; }

        public string? Notes { get; set; }

        public virtual ICollection<SalesQuotationItem> SalesQuotationItems { get; set; }
    }
}