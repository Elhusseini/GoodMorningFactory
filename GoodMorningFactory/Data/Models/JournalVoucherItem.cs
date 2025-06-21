// Data/Models/JournalVoucherItem.cs
// *** الكود الكامل لنموذج بنود القيد اليومي ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class JournalVoucherItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int JournalVoucherId { get; set; }
        public virtual JournalVoucher JournalVoucher { get; set; }

        [Required]
        public int AccountId { get; set; }
        public virtual Account Account { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Debit { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Credit { get; set; }

        public string? Description { get; set; }
        // --- بداية التحديث ---
        public bool IsReconciled { get; set; } = false;
        public int? BankReconciliationId { get; set; }
        public virtual BankReconciliation BankReconciliation { get; set; }
        // --- نهاية التحديث ---
    }
}