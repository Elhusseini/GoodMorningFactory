// Data/Models/BudgetDetail.cs
// *** ملف جديد: يمثل تفاصيل الموازنة التقديرية لحساب معين ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    [Table("BudgetDetails")]
    public class BudgetDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BudgetId { get; set; }
        public virtual Budget Budget { get; set; }

        [Required]
        public int AccountId { get; set; } // الحساب المراد وضع موازنة له (عادة حساب مصروف أو إيراد)
        public virtual Account Account { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Month1Amount { get; set; } // يناير

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Month2Amount { get; set; } // فبراير

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Month3Amount { get; set; } // مارس

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Month4Amount { get; set; } // أبريل

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Month5Amount { get; set; } // مايو

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Month6Amount { get; set; } // يونيو

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Month7Amount { get; set; } // يوليو

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Month8Amount { get; set; } // أغسطس

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Month9Amount { get; set; } // سبتمبر

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Month10Amount { get; set; } // أكتوبر

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Month11Amount { get; set; } // نوفمبر

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Month12Amount { get; set; } // ديسمبر
    }
}
