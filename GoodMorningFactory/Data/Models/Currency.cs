// Data/Models/Currency.cs
// *** تحديث: تمت إضافة حقول لدعم التفقيط بالعملة الافتراضية ***
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class Currency
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // مثال: "دينار كويتي"

        [Required]
        [MaxLength(5)]
        public string Code { get; set; } // مثال: "KWD"

        [Required]
        [MaxLength(5)]
        public string Symbol { get; set; } // مثال: "د.ك"

        public bool IsActive { get; set; } = true;

        public bool IsDefault { get; set; } = false;

        // --- بداية الإضافة: حقول التفقيط ---
        /// <summary>
        /// اسم العملة الرئيسية المستخدم في التفقيط (مثال: ريال سعودي)
        /// </summary>
        [MaxLength(50)]
        public string? CurrencyName_AR { get; set; }

        /// <summary>
        /// اسم وحدة الكسر المستخدم في التفقيط (مثال: هللة)
        /// </summary>
        [MaxLength(50)]
        public string? FractionalUnit_AR { get; set; }
        // --- نهاية الإضافة ---
    }
}
