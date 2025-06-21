// Data/Models/UnitOfMeasure.cs
// *** ملف جديد: يمثل جدول وحدات القياس الرئيسي ***
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class UnitOfMeasure
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // اسم الوحدة (مثال: قطعة، متر)

        [Required]
        [MaxLength(10)]
        public string Abbreviation { get; set; } // الاختصار (مثال: ق، م)
    }
}
