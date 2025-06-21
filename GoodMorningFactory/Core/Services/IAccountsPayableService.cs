using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف العقد الخاص بخدمات الذمم الدائنة.
    /// </summary>
    public interface IAccountsPayableService
    {
        /// <summary>
        /// جلب قائمة بفواتير الموردين المفتوحة مع حساب أعمار ديونها.
        /// </summary>
        Task<List<AccountsPayableViewModel>> GetAgingReportAsync();

        /// <summary>
        /// جلب تفاصيل فاتورة مورد معين لتسجيل دفعة.
        /// </summary>
        Task<PaymentDetailsDto> GetPaymentDetailsAsync(int purchaseId);

        /// <summary>
        /// تسجيل دفعة لفاتورة مورد وإنشاء قيد محاسبي.
        /// </summary>
        Task RecordPaymentAsync(int purchaseId, decimal amountPaid);
    }

    /// <summary>
    /// كائن لنقل تفاصيل الدفعة من الخدمة إلى ViewModel.
    /// </summary>

}