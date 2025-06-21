using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف العقد الخاص بخدمات الذمم المدينة.
    /// </summary>
    public interface IAccountsReceivableService
    {
        /// <summary>
        /// جلب قائمة بفواتير العملاء المفتوحة مع حساب أعمار ديونها.
        /// </summary>
        /// <returns>قائمة من نماذج العرض لتقرير أعمار الديون.</returns>
        Task<List<AccountsReceivableViewModel>> GetAgingReportAsync();
    }
}