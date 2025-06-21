// UI/ViewModels/FinancialDashboardViewModel.cs
// *** تحديث: تمت إضافة خصائص منسقة لجميع المؤشرات المالية ***
using LiveCharts;
using System.ComponentModel;
using GoodMorningFactory.Core.Services;

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

        // --- بداية التحديث: إضافة خصائص منسقة لجميع المؤشرات ---
        public string AccountsReceivableFormatted => $"{AccountsReceivable:N2} {AppSettings.DefaultCurrencySymbol}";
        public string AccountsPayableFormatted => $"{AccountsPayable:N2} {AppSettings.DefaultCurrencySymbol}";
        public string NetProfitLossYTDFormatted => $"{NetProfitLossYTD:N2} {AppSettings.DefaultCurrencySymbol}";
        public string TotalAssetsFormatted => $"{TotalAssets:N2} {AppSettings.DefaultCurrencySymbol}";
        public string TotalLiabilitiesFormatted => $"{TotalLiabilities:N2} {AppSettings.DefaultCurrencySymbol}";
        public string TotalEquityFormatted => $"{TotalEquity:N2} {AppSettings.DefaultCurrencySymbol}";
        // --- نهاية التحديث ---

        public SeriesCollection MonthlyPerformanceSeries { get; set; }
        public string[] MonthLabels { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
