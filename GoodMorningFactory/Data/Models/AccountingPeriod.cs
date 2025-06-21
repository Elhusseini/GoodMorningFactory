// Data/Models/AccountingPeriod.cs
// *** ملف جديد: يمثل فترة محاسبية محددة (شهر وسنة) وحالتها ***
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum PeriodStatus
    {
        Open,
        Closed
    }

    [Table("AccountingPeriods")]
    public class AccountingPeriod
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public PeriodStatus Status { get; set; } = PeriodStatus.Open;

        public DateTime? ClosedDate { get; set; }

        public int? ClosedByUserId { get; set; }
    }
}