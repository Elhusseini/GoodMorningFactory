// Data/Models/Employee.cs
// *** ملف جديد: يمثل جدول الموظفين الرئيسي مع جميع الحقول ***
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum Gender { Male, Female }
    public enum EmployeeStatus { Active, OnLeave, Terminated }

    public class Employee
    {
        [Key]
        public int Id { get; set; }

        // --- معلومات التوظيف ---
        [Required]
        [MaxLength(50)]
        public string EmployeeCode { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; } // تاريخ إنهاء الخدمة
        [MaxLength(100)]
        public string? JobTitle { get; set; }
        [MaxLength(100)]
        public string? Department { get; set; }
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        // --- معلومات شخصية ---
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        [MaxLength(100)]
        public string? MiddleName { get; set; }
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        [MaxLength(100)]
        public string? Nationality { get; set; }

        // --- معلومات الاتصال ---
        public string? Address { get; set; }
        [MaxLength(100)]
        public string? PersonalEmail { get; set; }
        [MaxLength(20)]
        public string? PersonalPhoneNumber { get; set; }

        // --- معلومات الراتب ---
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BasicSalary { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal HousingAllowance { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TransportationAllowance { get; set; }
    }
}
