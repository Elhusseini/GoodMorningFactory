// UI/ViewModels/DashboardViewModel.cs
// *** تحديث: تمت إضافة خصائص خاصة بالرسم البياني العمودي ***
using LiveCharts;

namespace GoodMorningFactory.UI.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalSalesToday { get; set; }
        public decimal TotalSalesThisMonth { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public SeriesCollection TopSellingProductsSeries { get; set; }

        // *** بداية التحديث ***
        public SeriesCollection MonthlySalesSeries { get; set; }
        public string[] MonthLabels { get; set; }
        // *** نهاية التحديث ***
    }
}