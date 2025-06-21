// Data/Models/Warehouse.cs
// *** ملف جديد: يمثل جدول المخازن الرئيسي ***
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } // كود المخزن

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } // اسم المخزن

        public string? Address { get; set; } // عنوان المخزن

        public bool IsActive { get; set; } = true;
    }
}