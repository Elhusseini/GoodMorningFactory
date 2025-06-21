using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف العمليات المتعلقة بجلب بيانات لوحة المعلومات المالية.
    /// </summary>
    public interface IFinancialDashboardService
    {
        /// <summary>
        /// جلب جميع المؤشرات المالية الرئيسية.
        /// </summary>
        Task<FinancialKpisDto> GetFinancialKpisAsync();

        /// <summary>
        /// جلب بيانات الأداء الشهري (الإيرادات مقابل المصروفات) لآخر 6 أشهر.
        /// </summary>
        Task<Dictionary<string, (decimal revenue, decimal expense)>> GetMonthlyPerformanceAsync();
    }

    /// <summary>
    /// كائن لنقل بيانات المؤشرات المالية الرئيسية من الخدمة إلى الـ ViewModel.
    /// </summary>
    public class FinancialKpisDto
    {
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal NetProfitLossYTD { get; set; }
        public decimal TotalEquity { get; set; }
        public decimal AccountsReceivable { get; set; }
        public decimal AccountsPayable { get; set; }
    }
}
