// Data/Models/LeaveType.cs
// *** ملف جديد: يمثل أنواع الإجازات المختلفة (سنوية, مرضية, إلخ) ***
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
    }
}