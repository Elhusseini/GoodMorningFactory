// Data/Models/Department.cs
// *** تحديث: تم تحديد اسم الجدول بشكل صريح باستخدام [Table] لمنع أخطاء التحديث ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // <-- تأكد من وجود هذا السطر

namespace GoodMorningFactory.Data.Models
{
    [Table("Departments")] // <-- هذا هو السطر المضاف لإصلاح المشكلة
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<User> Users { get; set; }
    }
}
