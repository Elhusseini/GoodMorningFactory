// Core/Services/ISettingsService.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة خدمة لإدارة جميع إعدادات التطبيق.
    /// توفر طرقًا لجلب وحفظ معلومات الشركة، الإعدادات العامة، الحسابات الافتراضية، وغيرها.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// جلب جميع معلومات وإعدادات الشركة من قاعدة البيانات.
        /// </summary>
        Task<CompanyInfo> GetCompanyInfoAsync();

        /// <summary>
        /// حفظ معلومات الشركة الأساسية.
        /// </summary>
        Task SaveCompanyInfoAsync(CompanyInfo info);

        /// <summary>
        /// حفظ الإعدادات العامة (اللغة، العملة، ...).
        /// </summary>
        Task SaveGeneralSettingsAsync(CompanyInfo info);

        /// <summary>
        /// حفظ إعدادات المستخدمين وسياسة كلمة المرور.
        /// </summary>
        Task SaveUserSettingsAsync(CompanyInfo info);

        /// <summary>
        /// حفظ الحسابات الافتراضية.
        /// </summary>
        Task SaveDefaultAccountsAsync(CompanyInfo info);

        /// <summary>
        /// حفظ إعدادات المخزون (طريقة التقييم).
        /// </summary>
        Task SaveInventorySettingsAsync(CompanyInfo info);

        /// <summary>
        /// حفظ إعدادات النسخ الاحتياطي التلقائي.
        /// </summary>
        Task SaveBackupSettingsAsync(CompanyInfo info);

        /// <summary>
        /// جلب قائمة بجميع الحسابات من شجرة الحسابات.
        /// </summary>
        Task<List<Account>> GetAccountsAsync();

        /// <summary>
        /// جلب قائمة بجميع العملات النشطة.
        /// </summary>
        Task<List<Currency>> GetActiveCurrenciesAsync();

        /// <summary>
        /// جلب قائمة بجميع أدوار المستخدمين.
        /// </summary>
        Task<List<Role>> GetRolesAsync();

        /// <summary>
        /// جلب إعدادات الترقيم التلقائي للمستندات.
        /// </summary>
        Task<List<NumberingSequence>> GetNumberingSequencesAsync();

        /// <summary>
        /// حفظ إعدادات الترقيم التلقائي.
        /// </summary>
        Task SaveNumberingSequencesAsync(IEnumerable<NumberingSequence> sequences);

        /// <summary>
        /// جلب إعدادات الإشعارات.
        /// </summary>
        Task<List<NotificationSetting>> GetNotificationSettingsAsync();

        /// <summary>
        /// حفظ إعدادات الإشعارات.
        /// </summary>
        Task SaveNotificationSettingsAsync(IEnumerable<NotificationSetting> settings);

        /// <summary>
        /// جلب قائمة بملفات النسخ الاحتياطي الموجودة.
        /// </summary>
        Task<List<BackupFileViewModel>> GetBackupFilesAsync();

        /// <summary>
        /// إنشاء نسخة احتياطية جديدة من قاعدة البيانات.
        /// </summary>
        Task CreateBackupAsync();

        /// <summary>
        /// استعادة قاعدة البيانات من ملف نسخة احتياطية.
        /// </summary>
        Task RestoreBackupAsync(string backupFilePath);

        /// <summary>
        /// حذف ملف نسخة احتياطية محدد.
        /// </summary>
        Task DeleteBackupAsync(string backupFilePath);
    }
}