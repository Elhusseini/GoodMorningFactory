// UI/ViewModels/FinancialDashboardViewModel.cs
// *** ملف جديد: ViewModel خاص ببيانات لوحة المعلومات المالية ***
using LiveCharts;
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class FinancialDashboardViewModel : INotifyPropertyChanged
    {
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public decimal NetProfitLossYTD { get; set; }
        public decimal AccountsReceivable { get; set; }
        public decimal AccountsPayable { get; set; }
        public SeriesCollection MonthlyPerformanceSeries { get; set; }
        public string[] MonthLabels { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}