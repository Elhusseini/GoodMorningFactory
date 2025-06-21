using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel للوحة التحكم الرئيسية.
    /// يعتمد على DashboardService لجلب البيانات ويقوم بتجهيزها للعرض.
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IDashboardService _dashboardService;

        #region Properties for KPI Cards
        private decimal _totalSalesToday;
        public decimal TotalSalesToday
        {
            get => _totalSalesToday;
            set { _totalSalesToday = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalSalesTodayFormatted)); }
        }

        private decimal _totalSalesThisMonth;
        public decimal TotalSalesThisMonth
        {
            get => _totalSalesThisMonth;
            set { _totalSalesThisMonth = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalSalesThisMonthFormatted)); }
        }

        private int _totalProducts;
        public int TotalProducts
        {
            get => _totalProducts;
            set { _totalProducts = value; OnPropertyChanged(); }
        }

        private int _lowStockProducts;
        public int LowStockProducts
        {
            get => _lowStockProducts;
            set { _lowStockProducts = value; OnPropertyChanged(); }
        }

        public string TotalSalesTodayFormatted => $"{TotalSalesToday:N2} {AppSettings.DefaultCurrencySymbol}";
        public string TotalSalesThisMonthFormatted => $"{TotalSalesThisMonth:N2} {AppSettings.DefaultCurrencySymbol}";
        #endregion

        #region Properties for Charts
        private SeriesCollection _monthlySalesSeries;
        public SeriesCollection MonthlySalesSeries
        {
            get => _monthlySalesSeries;
            set { _monthlySalesSeries = value; OnPropertyChanged(); }
        }

        private string[] _monthLabels;
        public string[] MonthLabels
        {
            get => _monthLabels;
            set { _monthLabels = value; OnPropertyChanged(); }
        }

        private SeriesCollection _topSellingProductsSeries;
        public SeriesCollection TopSellingProductsSeries
        {
            get => _topSellingProductsSeries;
            set { _topSellingProductsSeries = value; OnPropertyChanged(); }
        }

        public Func<double, string> SalesYFormatter { get; private set; }
        #endregion

        public ICommand RefreshCommand { get; }

        public DashboardViewModel()
        {
            _dashboardService = new DashboardService();
            SalesYFormatter = value => $"{value:N0} {AppSettings.DefaultCurrencySymbol}";
            RefreshCommand = new RelayCommand(async _ => await LoadDashboardDataAsync());

            LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                // تحميل بيانات المؤشرات الرئيسية بشكل متوازي لتحسين الأداء
                var salesTodayTask = _dashboardService.GetTotalSalesTodayAsync();
                var salesMonthTask = _dashboardService.GetTotalSalesThisMonthAsync();
                var totalProductsTask = _dashboardService.GetTotalProductsAsync();
                var lowStockTask = _dashboardService.GetLowStockProductsCountAsync();
                var monthlySalesTask = _dashboardService.GetMonthlySalesDataAsync();
                var topProductsTask = _dashboardService.GetTopSellingProductsAsync(5);

                await Task.WhenAll(salesTodayTask, salesMonthTask, totalProductsTask, lowStockTask, monthlySalesTask, topProductsTask);

                TotalSalesToday = await salesTodayTask;
                TotalSalesThisMonth = await salesMonthTask;
                TotalProducts = await totalProductsTask;
                LowStockProducts = await lowStockTask;

                // تجهيز بيانات الرسم البياني للمبيعات الشهرية
                var monthlySalesData = await monthlySalesTask;
                MonthLabels = monthlySalesData.Keys.ToArray();
                MonthlySalesSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "إجمالي المبيعات",
                        Values = new ChartValues<decimal>(monthlySalesData.Values)
                    }
                };

                // تجهيز بيانات الرسم البياني للمنتجات الأكثر مبيعاً
                var topProducts = await topProductsTask;
                TopSellingProductsSeries = new SeriesCollection();
                foreach (var product in topProducts)
                {
                    TopSellingProductsSeries.Add(new PieSeries
                    {
                        Title = product.ProductName,
                        Values = new ChartValues<int> { product.TotalQuantity },
                        DataLabels = true
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"حدث خطأ أثناء تحميل بيانات لوحة التحكم: {ex.Message}", "خطأ");
            }
        }
    }
}
