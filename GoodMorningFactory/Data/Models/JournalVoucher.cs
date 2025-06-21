// Data/Models/JournalVoucher.cs
// *** الكود الكامل لنموذج القيد اليومي الرئيسي ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    // --- بداية التحديث ---
    public enum VoucherStatus { Draft, Posted, Cancelled }
    // --- نهاية التحديث ---

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

        [Required]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalDebit { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCredit { get; set; }

        // --- بداية التحديث ---
        [Required]
        public VoucherStatus Status { get; set; } = VoucherStatus.Posted; // افتراضياً، يتم الترحيل مباشرة

        public bool IsReversed { get; set; } = false; // لتتبع ما إذا تم عكس القيد
        // --- نهاية التحديث ---

        public virtual ICollection<JournalVoucherItem> JournalVoucherItems { get; set; }
    }
}