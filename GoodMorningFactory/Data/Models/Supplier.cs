// GoodMorningFactory/Data/Models/Supplier.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    /// <summary>
    /// يمثل هذا الكلاس جدول الموردين في قاعدة البيانات.
    /// </summary>
    [Table("Suppliers")]
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "كود المورد مطلوب.")]
        [MaxLength(50)]
        public string SupplierCode { get; set; }

        [Required(ErrorMessage = "اسم المورد مطلوب.")]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string? ContactPerson { get; set; }

        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? TaxNumber { get; set; }

        [MaxLength(255)]
        public string? Website { get; set; }

        public string? Address { get; set; }

        public string? DefaultPaymentTerms { get; set; }

        public bool IsActive { get; set; } = true;

        // علاقات الارتباط مع الجداول الأخرى
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}
