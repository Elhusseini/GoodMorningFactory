// Data/Models/LeaveType.cs
// *** تحديث: تمت إضافة خاصية لتحديد نوع الإجازة (مدفوعة/غير مدفوعة) ***
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class LeaveType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // اسم نوع الإجازة

        public string? Description { get; set; }

        public int DaysPerYear { get; set; } // عدد الأيام المستحقة سنوياً

        // --- بداية التحديث ---
        public bool IsPaid { get; set; } = true; // هل الإجازة مدفوعة الأجر؟
        // --- نهاية التحديث ---
    }
}
