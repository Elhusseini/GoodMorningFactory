// Data/Models/Budget.cs
// *** ملف جديد: يمثل رأس الموازنة التقديرية ***
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    [Table("Budgets")]
    public class Budget
    {
        public Budget()
        {
            BudgetDetails = new HashSet<BudgetDetail>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } // مثال: "الموازنة الرئيسية لعام 2025"

        [Required]
        public int Year { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<BudgetDetail> BudgetDetails { get; set; }
    }
}
