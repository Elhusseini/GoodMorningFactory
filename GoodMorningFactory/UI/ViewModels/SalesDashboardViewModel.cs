// UI/ViewModels/SalesDashboardViewModel.cs
// *** الكود الكامل لـ ViewModel الخاص ببيانات لوحة معلومات المبيعات ***
using LiveCharts;
using System.Collections.Generic;

namespace GoodMorningFactory.UI.ViewModels
{
    public class SalesDashboardViewModel
    {
        // مؤشرات الأداء الرئيسية
        public decimal TotalSalesThisMonth { get; set; }
        public int NewOrdersThisMonth { get; set; }
        public int FollowUpQuotationsCount { get; set; }
        public decimal AverageOrderValue { get; set; }

        // قوائم الأفضل
        public List<string> TopCustomers { get; set; }
        public List<string> TopProducts { get; set; }

        // بيانات قمع المبيعات
        public int QuotationsCount { get; set; }
        public int OrdersCount { get; set; }
        public int InvoicesCount { get; set; }

        // بيانات الرسوم البيانية
        public SeriesCollection MonthlySalesSeries { get; set; }
        public string[] MonthLabels { get; set; }
        public SeriesCollection SalesByCategorySeries { get; set; }
        public string[] CategoryLabels { get; set; }

        public SalesDashboardViewModel()
        {
            TopCustomers = new List<string>();
            TopProducts = new List<string>();
        }
    }
}