using GoodMorningFactory.Core.Services;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel للوحة المعلومات المالية.
    /// يعتمد على FinancialDashboardService لجلب البيانات ويقوم بتجهيزها للعرض.
    /// </summary>
    public class FinancialDashboardViewModel : BaseViewModel
    {
        private readonly IFinancialDashboardService _financialDashboardService;

        #region Properties for KPI Cards
        private decimal _accountsReceivable;
        public decimal AccountsReceivable
        {
            get => _accountsReceivable;
            set { _accountsReceivable = value; OnPropertyChanged(); OnPropertyChanged(nameof(AccountsReceivableFormatted)); }
        }

        private decimal _accountsPayable;
        public decimal AccountsPayable
        {
            get => _accountsPayable;
            set { _accountsPayable = value; OnPropertyChanged(); OnPropertyChanged(nameof(AccountsPayableFormatted)); }
        }

        private decimal _netProfitLossYTD;
        public decimal NetProfitLossYTD
        {
            get => _netProfitLossYTD;
            set { _netProfitLossYTD = value; OnPropertyChanged(); OnPropertyChanged(nameof(NetProfitLossYTDFormatted)); }
        }

        private decimal _totalAssets;
        public decimal TotalAssets
        {
            get => _totalAssets;
            set { _totalAssets = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalAssetsFormatted)); }
        }

        private decimal _totalLiabilities;
        public decimal TotalLiabilities
        {
            get => _totalLiabilities;
            set { _totalLiabilities = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalLiabilitiesFormatted)); }
        }

        private decimal _totalEquity;
        public decimal TotalEquity
        {
            get => _totalEquity;
            set { _totalEquity = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalEquityFormatted)); }
        }

        // الخصائص المنسقة لعرض العملة بشكل صحيح
        public string AccountsReceivableFormatted => $"{AccountsReceivable:N2} {AppSettings.DefaultCurrencySymbol}";
        public string AccountsPayableFormatted => $"{AccountsPayable:N2} {AppSettings.DefaultCurrencySymbol}";
        public string NetProfitLossYTDFormatted => $"{NetProfitLossYTD:N2} {AppSettings.DefaultCurrencySymbol}";
        public string TotalAssetsFormatted => $"{TotalAssets:N2} {AppSettings.DefaultCurrencySymbol}";
        public string TotalLiabilitiesFormatted => $"{TotalLiabilities:N2} {AppSettings.DefaultCurrencySymbol}";
        public string TotalEquityFormatted => $"{TotalEquity:N2} {AppSettings.DefaultCurrencySymbol}";
        #endregion

        #region Properties for Charts
        private SeriesCollection _monthlyPerformanceSeries;
        public SeriesCollection MonthlyPerformanceSeries
        {
            get => _monthlyPerformanceSeries;
            set { _monthlyPerformanceSeries = value; OnPropertyChanged(); }
        }

        private string[] _monthLabels;
        public string[] MonthLabels
        {
            get => _monthLabels;
            set { _monthLabels = value; OnPropertyChanged(); }
        }

        public Func<double, string> YAxisFormatter { get; private set; }
        #endregion

        public FinancialDashboardViewModel()
        {
            _financialDashboardService = new FinancialDashboardService();
            YAxisFormatter = value => $"{value:N0} {AppSettings.DefaultCurrencySymbol}";
            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var kpisTask = _financialDashboardService.GetFinancialKpisAsync();
                var monthlyPerformanceTask = _financialDashboardService.GetMonthlyPerformanceAsync();

                await Task.WhenAll(kpisTask, monthlyPerformanceTask);

                var kpis = await kpisTask;
                AccountsReceivable = kpis.AccountsReceivable;
                AccountsPayable = kpis.AccountsPayable;
                NetProfitLossYTD = kpis.NetProfitLossYTD;
                TotalAssets = kpis.TotalAssets;
                TotalLiabilities = kpis.TotalLiabilities;
                TotalEquity = kpis.TotalEquity;

                var monthlyData = await monthlyPerformanceTask;
                MonthLabels = monthlyData.Keys.ToArray();
                MonthlyPerformanceSeries = new SeriesCollection
                {
                    new ColumnSeries { Title = "الإيرادات", Values = new ChartValues<decimal>(monthlyData.Values.Select(v => v.revenue)) },
                    new ColumnSeries { Title = "المصروفات", Values = new ChartValues<decimal>(monthlyData.Values.Select(v => v.expense)) }
                };
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"حدث خطأ أثناء تحميل بيانات لوحة المعلومات المالية: {ex.Message}", "خطأ");
            }
        }
    }
}
