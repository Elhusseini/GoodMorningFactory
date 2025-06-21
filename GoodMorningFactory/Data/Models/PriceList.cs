// Data/Models/PriceList.cs
// *** ملف جديد: يمثل جدول قوائم الأسعار الرئيسي ***
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class PriceList
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // اسم قائمة الأسعار (مثال: سعر التجزئة)

        public string? Description { get; set; }
    }
}