// UI/ViewModels/CostCenterReportViewModel.cs
// *** ملف جديد: ViewModel خاص ببيانات تقرير مراكز التكلفة ***
using GoodMorningFactory.Core.Services;

namespace GoodMorningFactory.UI.ViewModels
{
    public class CostCenterReportViewModel
    {
        /// <summary>
        /// اسم مركز التكلفة
        /// </summary>
        public string CostCenterName { get; set; }

        /// <summary>
        /// إجمالي الإيرادات المرتبطة بمركز التكلفة
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// إجمالي المصروفات المرتبطة بمركز التكلفة
        /// </summary>
        public decimal TotalExpenses { get; set; }

        /// <summary>
        /// صافي الربح أو الخسارة لمركز التكلفة
        /// </summary>
        public decimal NetProfitLoss => TotalRevenue - TotalExpenses;

        // --- خصائص منسقة للعرض في الواجهة مع رمز العملة ---
        public string TotalRevenueFormatted => $"{TotalRevenue:N2} {AppSettings.DefaultCurrencySymbol}";
        public string TotalExpensesFormatted => $"{TotalExpenses:N2} {AppSettings.DefaultCurrencySymbol}";
        public string NetProfitLossFormatted => $"{NetProfitLoss:N2} {AppSettings.DefaultCurrencySymbol}";
    }
}
