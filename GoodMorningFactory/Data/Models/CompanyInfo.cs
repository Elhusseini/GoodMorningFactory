// GoodMorningFactory/Data/Models/CompanyInfo.cs
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class CompanyInfo
    {
        [Key]
        public int Id { get; set; }

        // --- معلومات المصنع الأساسية ---
        public string? CompanyName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? TaxNumber { get; set; } // الرقم الضريبي
        public string? CommercialRegistrationNumber { get; set; } // رقم السجل التجاري
        public byte[]? Logo { get; set; } // الشعار يتم تخزينه كمصفوفة من البايتات

        // --- الحسابات الافتراضية: تستخدم لإنشاء القيود المحاسبية تلقائياً ---
        public int? DefaultSalesAccountId { get; set; } // حساب المبيعات
        public int? DefaultAccountsReceivableAccountId { get; set; } // حساب الذمم المدينة
        public int? DefaultPurchasesAccountId { get; set; } // حساب المشتريات
        public int? DefaultAccountsPayableAccountId { get; set; } // حساب الذمم الدائنة
        public int? DefaultCashAccountId { get; set; } // حساب النقدية أو البنك
        public int? DefaultInventoryAccountId { get; set; } // حساب المخزون
        public int? DefaultPurchaseReturnsAccountId { get; set; } // حساب مرتجعات المشتريات
        public int? DefaultCogsAccountId { get; set; } // حساب تكلفة البضاعة المباعة
        public int? DefaultPayrollExpenseAccountId { get; set; } // حساب مصروف الرواتب
        public int? DefaultPayrollAccrualAccountId { get; set; } // حساب الرواتب المستحقة
        public int? DefaultVatAccountId { get; set; } // حساب ضريبة القيمة المضافة
        public int? DefaultInventoryAdjustmentAccountId { get; set; } // حساب تسوية المخزون
        public int? DefaultGoodReceiptsAccrualAccountId { get; set; } // حساب وسيط (بضاعة مستلمة غير مفوترة)
        public int? DefaultSalesReturnsAccountId { get; set; }

        // --- بداية الإضافة: الحقل المفقود الذي كان يسبب الخطأ ---
        /// <summary>
        /// حساب الإنتاج تحت التشغيل (Work-in-Progress)
        /// وهو حساب وسيط تُجمع فيه تكاليف الإنتاج (مواد وعمالة).
        /// </summary>
        public int? DefaultWipAccountId { get; set; }
        // --- نهاية الإضافة ---

        // --- الإعدادات العامة للنظام ---
        public string? DefaultLanguage { get; set; }
        public string? DefaultDateFormat { get; set; }
        public int? DefaultCurrencyId { get; set; }
        public virtual Currency? DefaultCurrency { get; set; } // علاقة مع جدول العملات
        public InventoryCostingMethod DefaultCostingMethod { get; set; } = InventoryCostingMethod.WeightedAverage; // طريقة تقييم المخزون الافتراضية

        // --- إعدادات النسخ الاحتياطي التلقائي ---
        public bool IsAutoBackupEnabled { get; set; } = true;
        public int BackupsToKeep { get; set; } = 7;

        // --- إعدادات سياسة كلمات المرور للمستخدمين ---
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