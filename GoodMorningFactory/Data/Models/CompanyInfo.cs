// Data/Models/CompanyInfo.cs
// *** تحديث: إضافة إعدادات النسخ الاحتياطي التلقائي ***
using System.ComponentModel.DataAnnotations;

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
        public int? DefaultAccountsReceivableAccountId { get; set; }
        public int? DefaultPurchasesAccountId { get; set; }
        public int? DefaultAccountsPayableAccountId { get; set; }
        public int? DefaultCashAccountId { get; set; }
        public int? DefaultInventoryAccountId { get; set; }
        public int? DefaultPurchaseReturnsAccountId { get; set; }
        public int? DefaultCogsAccountId { get; set; }
        public int? DefaultPayrollExpenseAccountId { get; set; }
        public int? DefaultPayrollAccrualAccountId { get; set; }
        public int? DefaultVatAccountId { get; set; }
        public int? DefaultInventoryAdjustmentAccountId { get; set; }

        // --- الإعدادات العامة ---
        public string? DefaultLanguage { get; set; }
        public string? DefaultDateFormat { get; set; }
        public int? DefaultCurrencyId { get; set; }
        public virtual Currency? DefaultCurrency { get; set; }
        public InventoryCostingMethod DefaultCostingMethod { get; set; } = InventoryCostingMethod.WeightedAverage;

        // --- بداية الإضافة: إعدادات النسخ الاحتياطي التلقائي ---
        public bool IsAutoBackupEnabled { get; set; } = true; // تفعيل الميزة افتراضياً
        public int BackupsToKeep { get; set; } = 7; // الاحتفاظ بآخر 7 نسخ افتراضياً
        // --- نهاية الإضافة ---

        // --- إعدادات المستخدمين ---
        public int MinPasswordLength { get; set; } = 8;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool RequireSpecialChar { get; set; } = false;
        public int PasswordExpiryDays { get; set; } = 90;
        public int FailedLoginLockoutAttempts { get; set; } = 5;
        public int? DefaultRoleId { get; set; }
    }
}
