using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Linq; // <-- إضافة using
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel الرئيسي لشاشة عرض الشحنات.
    /// </summary>
    public class ShipmentsViewViewModel : BaseViewModel
    {
        private readonly IShipmentService _shipmentService;
        private readonly IFilterService _filterService;
        private readonly IPrintingService _printingService;

        #region Properties
        private ObservableCollection<ShipmentViewModel> _shipments;
        public ObservableCollection<ShipmentViewModel> Shipments { get => _shipments; set { _shipments = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ResetAndLoad(); } }

        public ObservableCollection<FilterItem<ShipmentStatus?>> StatusFilters { get; private set; }
        private FilterItem<ShipmentStatus?> _selectedStatusFilter;
        public FilterItem<ShipmentStatus?> SelectedStatusFilter { get => _selectedStatusFilter; set { _selectedStatusFilter = value; OnPropertyChanged(); ResetAndLoad(); } }

        private DateTime? _fromDate;
        public DateTime? FromDate { get => _fromDate; set { _fromDate = value; OnPropertyChanged(); ResetAndLoad(); } }

        private DateTime? _toDate;
        public DateTime? ToDate { get => _toDate; set { _toDate = value; OnPropertyChanged(); ResetAndLoad(); } }

        private string _pageInfo;
        public string PageInfo { get => _pageInfo; set { _pageInfo = value; OnPropertyChanged(); } }

        private int _currentPage = 1;
        private readonly int _pageSize = 15;
        private int _totalItems = 0;
        #endregion

        #region Commands
        public ICommand EditShipmentCommand { get; }
        public ICommand PrintPackingSlipCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        #endregion

        public ShipmentsViewViewModel()
        {
            _shipmentService = new ShipmentService();
            _filterService = new FilterService();
            _printingService = new PrintingService();

            StatusFilters = new ObservableCollection<FilterItem<ShipmentStatus?>>(_filterService.GetShipmentStatusFilters());
            _selectedStatusFilter = StatusFilters[0];

            EditShipmentCommand = new RelayCommand(EditShipment, CanActOnShipment);
            PrintPackingSlipCommand = new RelayCommand(PrintPackingSlip, CanActOnShipment);
            RefreshCommand = new RelayCommand(async _ => await LoadShipmentsAsync());
            NextPageCommand = new RelayCommand(GoToNextPage, _ => _currentPage < GetTotalPages());
            PreviousPageCommand = new RelayCommand(GoToPreviousPage, _ => _currentPage > 1);

            LoadShipmentsAsync();
        }

        private async Task LoadShipmentsAsync()
        {
            try
            {
                var criteria = new ShipmentFilterCriteria
                {
                    Page = _currentPage,
                    PageSize = _pageSize,
                    SearchText = SearchText,
                    Status = _selectedStatusFilter?.Value,
                    FromDate = FromDate,
                    ToDate = ToDate
                };

                // 1. استدعاء الخدمة التي تُرجع نماذج البيانات الخام
                var result = await _shipmentService.GetShipmentsAsync(criteria);

                // 2. الـ ViewModel يقوم بتحويل البيانات إلى نماذج عرض
                var viewModels = result.Items.Select(s => new ShipmentViewModel
                {
                    Id = s.Id,
                    ShipmentNumber = s.ShipmentNumber,
                    ShipmentDate = s.ShipmentDate,
                    SalesOrderNumber = s.SalesOrder.SalesOrderNumber,
                    CustomerName = s.SalesOrder.Customer.CustomerName,
                    Carrier = s.Carrier,
                    TrackingNumber = s.TrackingNumber,
                    Status = s.Status
                }).ToList();

                Shipments = new ObservableCollection<ShipmentViewModel>(viewModels);
                _totalItems = result.TotalCount;
                UpdatePageInfo();
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل الشحنات: {ex.Message}", "خطأ"); }
        }

        private void EditShipment(object parameter)
        {
            if (parameter is ShipmentViewModel shipment)
            {
                // الآن يتم استدعاء النافذة الجديدة التي تعمل بنمط MVVM
                var editWindow = new EditShipmentWindow(shipment.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadShipmentsAsync();
                }
            }
        }

        private async void PrintPackingSlip(object parameter)
        {
            if (parameter is ShipmentViewModel shipmentVM)
            {
                try
                {
                    var shipmentToPrint = await _shipmentService.GetShipmentForPackingSlipAsync(shipmentVM.Id);
                    await _printingService.PrintPackingSlipAsync(shipmentToPrint);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشلت عملية الطباعة: {ex.Message}", "خطأ");
                }
            }
        }

        private bool CanActOnShipment(object parameter) => parameter is ShipmentViewModel;

        #region Pagination
        private async void ResetAndLoad() { _currentPage = 1; await LoadShipmentsAsync(); }
        private async void GoToNextPage(object p) { if (_currentPage < GetTotalPages()) { _currentPage++; await LoadShipmentsAsync(); } }
        private async void GoToPreviousPage(object p) { if (_currentPage > 1) { _currentPage--; await LoadShipmentsAsync(); } }
        private int GetTotalPages() => (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / _pageSize);
        private void UpdatePageInfo() => PageInfo = $"الصفحة {_currentPage} من {GetTotalPages()} (إجمالي الشحنات: {_totalItems})";
        #endregion
    }
}
