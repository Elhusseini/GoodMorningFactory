// Data/Models/Budget.cs
// *** الكود الكامل والمؤكد ***
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
        public string Name { get; set; }

        [Required]
        public int Year { get; set; }

        // تم تعديل هذا الحقل ليكون مطلوبًا ويتوافق مع قاعدة البيانات
        // مع قيمة افتراضية لمنع الأخطاء عند إنشاء كائن جديد
        [Required]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<BudgetDetail> BudgetDetails { get; set; }
    }
}