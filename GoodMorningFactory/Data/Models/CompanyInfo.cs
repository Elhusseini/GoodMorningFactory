// Data/Models/CompanyInfo.cs
// *** تحديث: تمت إضافة حقول للحسابات الافتراضية ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class CompanyInfo
    {
        [Key]
        public int Id { get; set; }

        // --- معلومات المصنع ---
        public string? CompanyName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? TaxNumber { get; set; }
        public string? CommercialRegistrationNumber { get; set; }
        public byte[]? Logo { get; set; }

        // --- الحسابات الافتراضية ---
        public int? DefaultSalesAccountId { get; set; }
        [ForeignKey("DefaultSalesAccountId")]
        public virtual Account? DefaultSalesAccount { get; set; }

        // المفتاح الأجنبي لحساب العملاء (الذمم المدينة) الافتراضي
        public int? DefaultAccountsReceivableAccountId { get; set; }
        [ForeignKey("DefaultAccountsReceivableAccountId")]
        public virtual Account? DefaultAccountsReceivableAccount { get; set; }
        // --- نهاية التحديث ---
        // --- بداية التحديث: إضافة الحسابات الافتراضية للمشتريات ---
        public int? DefaultPurchasesAccountId { get; set; }
        public int? DefaultAccountsPayableAccountId { get; set; }

        // --- الإعدادات العامة ---
        public string? DefaultLanguage { get; set; }
        public string? DefaultDateFormat { get; set; }
        public string? BaseCurrency { get; set; }

        // --- بداية التحديث: إضافة حقول إعدادات المستخدمين ---
        public int MinPasswordLength { get; set; } = 8;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool RequireSpecialChar { get; set; } = false;
        public int PasswordExpiryDays { get; set; } = 90; // 0 for no expiry
        public int FailedLoginLockoutAttempts { get; set; } = 5;
        public int? DefaultRoleId { get; set; }
        // --- نهاية التحديث ---

    }
}