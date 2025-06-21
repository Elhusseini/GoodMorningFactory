// Data/Models/Lead.cs
// ملف جديد: يمثل عميل محتمل (Lead)
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum LeadStatus
    {
        [Description("جديد")]
        New,
        [Description("تم التواصل")]
        Contacted,
        [Description("مؤهل")]
        Qualified,
        [Description("غير مؤهل")]
        Unqualified
    }

    [Table("Leads")]
    public class Lead
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string CompanyName { get; set; }

        [MaxLength(100)]
        public string ContactPerson { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        public LeadStatus Status { get; set; } = LeadStatus.New;

        public string Source { get; set; } // مصدر العميل (مثال: معرض، موقع إلكتروني)

        public int? AssignedToUserId { get; set; } // الموظف المسؤول
        public virtual User AssignedToUser { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}


