// GoodMorningFactory/Data/Models/JournalVoucherItem.cs
// *** الكود الكامل والنهائي بعد المراجعة والتأكيد ***
using System;
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

        // --- حقول التسوية البنكية ---

        /// <summary>
        /// يشير إلى ما إذا كانت هذه الحركة قد تمت تسويتها.
        /// </summary>
        public bool IsReconciled { get; set; } = false;

        /// <summary>
        /// تاريخ إتمام التسوية لهذه الحركة.
        /// </summary>
        public DateTime? ReconciliationDate { get; set; }

        /// <summary>
        /// معرّف سجل التسوية الذي تنتمي إليه هذه الحركة.
        /// </summary>
        public int? BankReconciliationId { get; set; }
        public virtual BankReconciliation BankReconciliation { get; set; }

        // --- الربط بمركز التكلفة ---

        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }
    }
}