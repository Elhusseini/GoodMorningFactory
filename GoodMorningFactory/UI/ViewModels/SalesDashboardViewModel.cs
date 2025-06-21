// UI/ViewModels/SalesDashboardViewModel.cs
// *** تحديث: تمت إضافة خصائص قمع المبيعات ***
using GoodMorningFactory.Core.Services;
using LiveCharts;
using System.Collections.Generic;
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class SalesDashboardViewModel : INotifyPropertyChanged
    {
        // الخصائص الأصلية التي يتم تعبئتها من قاعدة البيانات
        public decimal TotalSalesThisMonth { get; set; }
        public int NewOrdersThisMonth { get; set; }
        public int FollowUpQuotationsCount { get; set; }
        public decimal AverageOrderValue { get; set; }

        // --- خصائص جديدة منسقة للعرض في الواجهة ---
        public string TotalSalesThisMonthFormatted => $"{TotalSalesThisMonth:N0} {AppSettings.DefaultCurrencySymbol}";
        public string AverageOrderValueFormatted => $"{AverageOrderValue:N0} {AppSettings.DefaultCurrencySymbol}";

        // القوائم
        public List<string> TopCustomers { get; set; }
        public List<string> TopProducts { get; set; }

        // --- بداية التحديث: إضافة خصائص قمع المبيعات ---
        public int QuotationsCount { get; set; }
        public int OrdersCount { get; set; }
        public int InvoicesCount { get; set; }
        // --- نهاية التحديث ---

        // الرسوم البيانية
        public SeriesCollection MonthlySalesSeries { get; set; }
        public string[] MonthLabels { get; set; }
        public SeriesCollection SalesByCategorySeries { get; set; }
        public string[] CategoryLabels { get; set; }

        public SalesDashboardViewModel()
        {
            TopCustomers = new List<string>();
            TopProducts = new List<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        // (INotifyPropertyChanged implementation would be here if needed for dynamic updates)
    }
}
