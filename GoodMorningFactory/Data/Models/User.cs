// Data/Models/User.cs
// هذا الكلاس يمثل جدول المستخدمين في قاعدة البيانات.
// كل خاصية (Property) فيه تمثل عموداً في الجدول.
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace GoodMorningFactory.Data.Models
{
    public class User
    {
        [Key] // يحدد أن هذا الحقل هو المفتاح الأساسي للجدول
        public int Id { get; set; }

        [Required] // يحدد أن هذا الحقل مطلوب ولا يمكن أن يكون فارغاً
        [MaxLength(100)] // يحدد الطول الأقصى للنص
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; } // سيتم تخزين كلمة المرور مشفرة

        public int RoleId { get; set; } // مفتاح أجنبي يربط المستخدم بالدور الخاص به
        public Role Role { get; set; } // خاصية للتنقل (Navigation Property) لجلب بيانات الدور

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}