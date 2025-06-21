using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum VoucherStatus
    {
        [Description("مسودة")]
        Draft,
        [Description("مرحّل")]
        Posted,
        [Description("ملغي")]
        Cancelled,
        [Description("مقترح")]

        Proposed    // مقترح <--- أضف هذه القيمة

    }

    public class JournalVoucher
    {
        public JournalVoucher()
        {
            JournalVoucherItems = new HashSet<JournalVoucherItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string VoucherNumber { get; set; }

        [Required]
        public DateTime VoucherDate { get; set; }

        // ---  بداية التعديل الرئيسي  ---
        [Required]
        public string Description { get; set; } = string.Empty; // تم تعيين قيمة افتراضية
        // ---  نهاية التعديل الرئيسي  ---

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalDebit { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCredit { get; set; }

        [Required]
        public VoucherStatus Status { get; set; } = VoucherStatus.Posted;

        public bool IsReversed { get; set; } = false;

        public virtual ICollection<JournalVoucherItem> JournalVoucherItems { get; set; }
    }
}