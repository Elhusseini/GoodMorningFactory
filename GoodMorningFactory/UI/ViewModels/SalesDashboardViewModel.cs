// GoodMorningFactory/UI/ViewModels/SalesDashboardViewModel.cs
// *** الكود الكامل والشامل والنهائي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class SalesDashboardViewModel : BaseViewModel
    {
        private readonly ISalesDashboardService _dashboardService;

        #region Properties
        private decimal _totalSalesThisMonth;
        public decimal TotalSalesThisMonth { get => _totalSalesThisMonth; set { _totalSalesThisMonth = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalSalesThisMonthFormatted)); } }
        public string TotalSalesThisMonthFormatted => $"{TotalSalesThisMonth:N0} {AppSettings.DefaultCurrencySymbol}";

        private int _newOrdersThisMonth;
        public int NewOrdersThisMonth { get => _newOrdersThisMonth; set { _newOrdersThisMonth = value; OnPropertyChanged(); } }

        private int _followUpQuotationsCount;
        public int FollowUpQuotationsCount { get => _followUpQuotationsCount; set { _followUpQuotationsCount = value; OnPropertyChanged(); } }

        private decimal _averageOrderValue;
        public decimal AverageOrderValue { get => _averageOrderValue; set { _averageOrderValue = value; OnPropertyChanged(); OnPropertyChanged(nameof(AverageOrderValueFormatted)); } }
        public string AverageOrderValueFormatted => $"{AverageOrderValue:N0} {AppSettings.DefaultCurrencySymbol}";

        public ObservableCollection<string> TopCustomers { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> TopProducts { get; } = new ObservableCollection<string>();

        private int _quotationsCount;
        public int QuotationsCount { get => _quotationsCount; set { _quotationsCount = value; OnPropertyChanged(); } }

        private int _ordersCount;
        public int OrdersCount { get => _ordersCount; set { _ordersCount = value; OnPropertyChanged(); } }

        private int _invoicesCount;
        public int InvoicesCount { get => _invoicesCount; set { _invoicesCount = value; OnPropertyChanged(); } }

        public SeriesCollection MonthlySalesSeries { get; set; } = new SeriesCollection();
        public string[] MonthLabels { get; private set; }
        public SeriesCollection SalesByCategorySeries { get; set; } = new SeriesCollection();
        public string[] CategoryLabels { get; private set; }
        public Func<double, string> SalesAxisYFormatter { get; set; }
        #endregion

        #region Commands
        public ICommand AddCustomerCommand { get; }
        public ICommand AddQuotationCommand { get; }
        public ICommand AddOrderCommand { get; }
        #endregion

        public SalesDashboardViewModel()
        {
            _dashboardService = new SalesDashboardService();
            SalesAxisYFormatter = value => $"{value:N0} {AppSettings.DefaultCurrencySymbol}";

            AddCustomerCommand = new RelayCommand((p) => AppServices.NavigationService.NavigateTo("Customers"));
            AddQuotationCommand = new RelayCommand((p) => AppServices.NavigationService.NavigateTo("Quotations"));
            AddOrderCommand = new RelayCommand((p) => AppServices.NavigationService.NavigateTo("Orders"));

            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                var data = await _dashboardService.GetDashboardDataAsync();

                TotalSalesThisMonth = data.TotalSalesThisMonth;
                NewOrdersThisMonth = data.NewOrdersThisMonth;
                FollowUpQuotationsCount = data.FollowUpQuotationsCount;
                AverageOrderValue = data.AverageOrderValue;
                QuotationsCount = data.QuotationsCount;
                OrdersCount = data.OrdersCount;
                InvoicesCount = data.InvoicesCount;

                TopCustomers.Clear();
                data.TopCustomers.ForEach(c => TopCustomers.Add(c));

                TopProducts.Clear();
                data.TopProducts.ForEach(p => TopProducts.Add(p));

                MonthLabels = data.MonthlySales.Keys.ToArray();
                MonthlySalesSeries.Clear();
                MonthlySalesSeries.Add(new ColumnSeries { Title = "إجمالي المبيعات", Values = new ChartValues<decimal>(data.MonthlySales.Values) });

                CategoryLabels = data.SalesByCategory.Keys.ToArray();
                SalesByCategorySeries.Clear();
                SalesByCategorySeries.Add(new RowSeries { Title = "المبيعات", Values = new ChartValues<decimal>(data.SalesByCategory.Values) });

                OnPropertyChanged(nameof(MonthLabels));
                OnPropertyChanged(nameof(CategoryLabels));
                OnPropertyChanged(nameof(MonthlySalesSeries));
                OnPropertyChanged(nameof(SalesByCategorySeries));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل بيانات لوحة المعلومات: {ex.Message}", "خطأ");
            }
        }
    }
}