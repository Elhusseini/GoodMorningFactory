// Data/Models/Account.cs
// *** تحديث: تمت إضافة حقول جديدة لدعم الهيكل الشجري والحالة ***
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    // أنواع الحسابات الرئيسية
    public enum AccountType { Asset, Liability, Equity, Revenue, Expense }

    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string AccountNumber { get; set; } // رقم الحساب

        [Required]
        [MaxLength(200)]
        public string AccountName { get; set; } // اسم الحساب

        [Required]
        public AccountType AccountType { get; set; } // نوع الحساب

        public int? ParentAccountId { get; set; } // المفتاح الأجنبي للحساب الأب
        [ForeignKey("ParentAccountId")]
        public virtual Account ParentAccount { get; set; }

        public virtual ICollection<Account> SubAccounts { get; set; } // قائمة الحسابات الفرعية
    }
}