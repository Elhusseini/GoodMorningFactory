// UI/ViewModels/InventoryDashboardViewModel.cs
using GoodMorningFactory.Core.Services;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.ObjectModel; // <-- إضافة مهمة
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class InventoryDashboardViewModel : BaseViewModel
    {
        private readonly IInventoryDashboardService _dashboardService;

        #region الخصائص (Properties)
        private decimal _totalInventoryValue;
        public decimal TotalInventoryValue
        {
            get => _totalInventoryValue;
            set { _totalInventoryValue = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalInventoryValueFormatted)); }
        }

        private int _lowStockItems;
        public int LowStockItems
        {
            get => _lowStockItems;
            set { _lowStockItems = value; OnPropertyChanged(); }
        }

        private int _outOfStockItems;
        public int OutOfStockItems
        {
            get => _outOfStockItems;
            set { _outOfStockItems = value; OnPropertyChanged(); }
        }

        public string TotalInventoryValueFormatted => $"{TotalInventoryValue:N2} {AppSettings.DefaultCurrencySymbol}";

        private SeriesCollection _valueByCategorySeries;
        public SeriesCollection ValueByCategorySeries
        {
            get => _valueByCategorySeries;
            set { _valueByCategorySeries = value; OnPropertyChanged(); }
        }

        // --- بداية الإضافة: خصائص جديدة للقوائم ---
        private ObservableCollection<StagnantProductDto> _stagnantProducts;
        public ObservableCollection<StagnantProductDto> StagnantProducts
        {
            get => _stagnantProducts;
            set { _stagnantProducts = value; OnPropertyChanged(); }
        }

        private ObservableCollection<TopValuedProductDto> _topValuedProducts;
        public ObservableCollection<TopValuedProductDto> TopValuedProducts
        {
            get => _topValuedProducts;
            set { _topValuedProducts = value; OnPropertyChanged(); }
        }
        // --- نهاية الإضافة ---

        #endregion

        public InventoryDashboardViewModel()
        {
            bool isInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());

            if (isInDesignMode)
            {
                LoadDesignTimeData();
            }
            else
            {
                _dashboardService = new InventoryDashboardService();
                LoadData();
            }
        }

        private async void LoadData()
        {
            if (_dashboardService == null) return;

            // استدعاء الخدمة التي تعيد DTO
            var data = await _dashboardService.GetDashboardDataAsync();
            if (data != null)
            {
                // الـ ViewModel يقوم بتعبئة خصائصه من الـ DTO
                TotalInventoryValue = data.TotalInventoryValue;
                LowStockItems = data.LowStockItems;
                OutOfStockItems = data.OutOfStockItems;
                ValueByCategorySeries = data.ValueByCategorySeries;

                // --- بداية الإضافة: تعبئة القوائم الجديدة ---
                StagnantProducts = new ObservableCollection<StagnantProductDto>(data.StagnantProducts);
                TopValuedProducts = new ObservableCollection<TopValuedProductDto>(data.TopValuedProducts);
                // --- نهاية الإضافة ---
            }
        }

        /// <summary>
        /// دالة خاصة لتحميل بيانات وهمية تظهر في المصمم فقط.
        /// </summary>
        private void LoadDesignTimeData()
        {
            TotalInventoryValue = 125300.50m;
            LowStockItems = 15;
            OutOfStockItems = 3;

            ValueByCategorySeries = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "مواد خام",
                    Values = new ChartValues<decimal> { 75000 },
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "منتجات نهائية",
                    Values = new ChartValues<decimal> { 50300.50m },
                    DataLabels = true
                }
            };

            // --- بداية الإضافة: بيانات وهمية للقوائم الجديدة ---
            StagnantProducts = new ObservableCollection<StagnantProductDto>
            {
                new StagnantProductDto { ProductName = "منتج راكد 1", DaysSinceLastMovement = 120, QuantityOnHand = 50 },
                new StagnantProductDto { ProductName = "منتج راكد 2", DaysSinceLastMovement = 95, QuantityOnHand = 200 }
            };
            TopValuedProducts = new ObservableCollection<TopValuedProductDto>
            {
                new TopValuedProductDto { ProductName = "منتج غالي 1", TotalValue = 50000, QuantityOnHand = 10 },
                new TopValuedProductDto { ProductName = "منتج غالي 2", TotalValue = 35000, QuantityOnHand = 500 }
            };
            // --- نهاية الإضافة ---
        }
    }
}