// Data/Models/BankReconciliation.cs
// *** ملف جديد: يمثل سجل تسوية بنكية واحد ***
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class BankReconciliation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BankAccountId { get; set; }

        [Required]
        public DateTime StatementDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal StatementEndingBalance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BookBalance { get; set; }
    }
}
