// GoodMorningFactory/Data/Models/Account.cs
// *** الكود الكامل والمؤكد ***
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum AccountType
    {
        [Description("أصل")]
        Asset,
        [Description("خصم (التزام)")]
        Liability,
        [Description("حقوق ملكية")]
        Equity,
        [Description("إيراد")]
        Revenue,
        [Description("مصروف")]
        Expense
    }

    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "رقم الحساب مطلوب.")]
        [MaxLength(50)]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "اسم الحساب مطلوب.")]
        [MaxLength(200)]
        public string AccountName { get; set; }

        [Required]
        public AccountType AccountType { get; set; }

        public string? Description { get; set; }

        public int? ParentAccountId { get; set; }
        [ForeignKey("ParentAccountId")]
        public virtual Account? ParentAccount { get; set; }
        public virtual ICollection<Account> ChildAccounts { get; set; } = new List<Account>();

        public bool IsPostingAccount { get; set; } = true;
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// خاصية لتحديد ما إذا كان هذا الحساب هو حساب بنكي.
        /// </summary>
        public bool IsBank { get; set; } = false;

        [NotMapped]
        public string DisplayName => $"{AccountNumber} - {AccountName}";

        [NotMapped]
        public decimal CurrentBalance { get; set; }
    }
}