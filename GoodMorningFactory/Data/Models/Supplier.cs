// Data/Models/Supplier.cs
// *** تحديث: تمت إضافة جميع الحقول الجديدة للمورد ***
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string SupplierCode { get; set; } // كود المورد

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } // اسم المورد

        [MaxLength(100)]
        public string? ContactPerson { get; set; } // اسم الشخص المسؤول

        [MaxLength(100)]
        public string? Email { get; set; } // البريد الإلكتروني

        [MaxLength(20)]
        public string? PhoneNumber { get; set; } // رقم الهاتف

        [MaxLength(100)]
        public string? TaxNumber { get; set; } // الرقم الضريبي

        [MaxLength(255)]
        public string? Website { get; set; } // الموقع الإلكتروني

        public string? Address { get; set; } // العنوان

        public string? DefaultPaymentTerms { get; set; } // شروط الدفع الافتراضية

        public bool IsActive { get; set; } = true; // حالة المورد
    }
}