// GoodMorningFactory/Data/Models/Department.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    /// <summary>
    /// يمثل هذا الكلاس جدول الأقسام في قاعدة البيانات.
    /// تم تحديد اسم الجدول بشكل صريح باستخدام [Table] لضمان التوافق.
    /// </summary>
    [Table("Departments")]
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم القسم مطلوب.")]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // علاقة ارتباط مع جدول المستخدمين
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
