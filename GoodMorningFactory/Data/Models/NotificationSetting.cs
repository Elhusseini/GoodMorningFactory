// Data/Models/NotificationSetting.cs
// *** ملف جديد: يمثل إعدادات الإشعار لحدث معين ***
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class NotificationSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string EventName { get; set; } // اسم الحدث البرمجي

        [Required]
        public string Description { get; set; } // الوصف الذي يظهر للمستخدم

        public bool IsEnabled { get; set; } = true; // تفعيل/إلغاء تفعيل الإشعار بشكل عام

        public bool SendInApp { get; set; } = true; // إرسال إشعار داخل التطبيق

        public bool SendEmail { get; set; } = false; // إرسال إشعار عبر البريد الإلكتروني
    }
}