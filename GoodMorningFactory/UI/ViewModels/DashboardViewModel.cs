// UI/ViewModels/DashboardViewModel.cs
// *** تحديث: تمت إضافة خصائص منسقة لعرض المبالغ بالعملة الافتراضية ***
using LiveCharts;
using GoodMorningFactory.Core.Services;

namespace GoodMorningFactory.UI.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalSalesToday { get; set; }
        public decimal TotalSalesThisMonth { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }

        // --- بداية التحديث ---
        public string TotalSalesTodayFormatted => $"{TotalSalesToday:N0} {AppSettings.DefaultCurrencySymbol}";
        public string TotalSalesThisMonthFormatted => $"{TotalSalesThisMonth:N0} {AppSettings.DefaultCurrencySymbol}";
        // --- نهاية التحديث ---

        public SeriesCollection TopSellingProductsSeries { get; set; }
        public SeriesCollection MonthlySalesSeries { get; set; }
        public string[] MonthLabels { get; set; }
    }
}
