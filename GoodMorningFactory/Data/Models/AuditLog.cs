// Data/Models/AuditLog.cs
// ملف جديد: يمثل هذا الكلاس جدول سجلات التدقيق في قاعدة البيانات.
// سيتم إنشاء سجل جديد في هذا الجدول مع كل عملية إضافة أو تعديل أو حذف هامة في النظام.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    // تعريف أنواع الإجراءات التي يمكن تسجيلها
    public enum AuditActionType
    {
        Create,
        Update,
        Delete
    }

    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        // هوية المستخدم الذي قام بالإجراء
        public int? UserId { get; set; }

        // اسم المستخدم لتسهيل العرض في التقارير
        [MaxLength(100)]
        public string Username { get; set; }

        // نوع الإجراء (إضافة, تعديل, حذف)
        [Required]
        public AuditActionType ActionType { get; set; }

        // اسم الجدول الذي تم عليه الإجراء (مثال: "Products", "Customers")
        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; }

        // المفتاح الأساسي للسجل الذي تم تغييره (مثال: هوية المنتج أو العميل)
        [Required]
        [MaxLength(100)]
        public string EntityKey { get; set; }

        // وصف للتغييرات التي حدثت (مثال: "تم تغيير حقل 'السعر' من 150 إلى 155")
        public string Changes { get; set; }

        // تاريخ ووقت حدوث الإجراء
        [Required]
        public DateTime Timestamp { get; set; }
    }
}
