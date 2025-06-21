// GoodMorningFactory/UI/ViewModels/SalesOrdersViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class SalesOrdersViewModel : BaseViewModel
    {
        private readonly ISalesOrderService _salesOrderService;
        private readonly IFilterService _filterService;
        private bool _isInitialized = false; // <-- متغير لمنع التحميل المتكرر

        #region Properties
        private ObservableCollection<SalesOrderViewModel> _orders;
        public ObservableCollection<SalesOrderViewModel> Orders { get => _orders; set { _orders = value; OnPropertyChanged(); } }

        private string _searchText = "";
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ResetAndLoad(); } }

        public ObservableCollection<FilterItem<OrderStatus?>> StatusFilters { get; }
        private FilterItem<OrderStatus?> _selectedStatusFilter;
        public FilterItem<OrderStatus?> SelectedStatusFilter { get => _selectedStatusFilter; set { _selectedStatusFilter = value; OnPropertyChanged(); ResetAndLoad(); } }

        private string _pageInfo;
        public string PageInfo { get => _pageInfo; set { _pageInfo = value; OnPropertyChanged(); } }

        private int _currentPage = 1;
        private readonly int _pageSize = 15;
        private int _totalItems = 0;
        #endregion

        #region Commands
        public ICommand LoadDataCommand { get; } // <-- الأمر الجديد
        public ICommand AddOrderCommand { get; }
        public ICommand EditOrderCommand { get; }
        public ICommand CancelOrderCommand { get; }
        public ICommand CreateWorkOrderCommand { get; }
        public ICommand CreateShipmentCommand { get; }
        public ICommand CreateInvoiceCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        #endregion

        public SalesOrdersViewModel()
        {
            _salesOrderService = new SalesOrderService();
            _filterService = new FilterService();
            StatusFilters = new ObservableCollection<FilterItem<OrderStatus?>>(_filterService.GetOrderStatusFilters());
            _selectedStatusFilter = StatusFilters.First();

            // --- بداية التعديل: ربط الأوامر بالدوال ---
            LoadDataCommand = new RelayCommand(async _ => await InitializeAsync());
            AddOrderCommand = new RelayCommand(AddOrder);
            EditOrderCommand = new RelayCommand(EditOrder, CanEditOrder);
            CancelOrderCommand = new RelayCommand(CancelOrder, CanCancelOrder);
            CreateWorkOrderCommand = new RelayCommand(CreateWorkOrder, CanCreateWorkOrder);
            CreateShipmentCommand = new RelayCommand(CreateShipment, CanCreateShipment);
            CreateInvoiceCommand = new RelayCommand(CreateInvoice, CanCreateInvoice);
            RefreshCommand = new RelayCommand(async _ => await LoadOrdersAsync());
            NextPageCommand = new RelayCommand(GoToNextPage, _ => _currentPage < GetTotalPages());
            PreviousPageCommand = new RelayCommand(GoToPreviousPage, _ => _currentPage > 1);
            // --- نهاية التعديل: تم حذف استدعاء LoadOrdersAsync() من هنا ---
        }

        /// <summary>
        /// هذه الدالة تُستدعى مرة واحدة فقط عند تحميل الواجهة لأول مرة.
        /// </summary>
        private async Task InitializeAsync()
        {
            if (_isInitialized) return;
            await LoadOrdersAsync();
            _isInitialized = true;
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                var result = await _salesOrderService.GetSalesOrdersAsync(_currentPage, _pageSize, SearchText, _selectedStatusFilter.Value);
                var viewModels = result.Items.Select(order => new SalesOrderViewModel { Order = order }).ToList();
                Orders = new ObservableCollection<SalesOrderViewModel>(viewModels);

                _totalItems = result.TotalCount;
                UpdatePageInfo();
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل أوامر البيع: {ex.Message}", "خطأ"); }
        }

        #region Command Implementations
        private void AddOrder(object parameter)
        {
            var addWindow = new AddEditSalesOrderWindow();
            if (addWindow.ShowDialog() == true) ResetAndLoad();
        }

        private void EditOrder(object parameter)
        {
            if (parameter is SalesOrderViewModel vm)
            {
                var editWindow = new AddEditSalesOrderWindow(vm.Id);
                if (editWindow.ShowDialog() == true) LoadOrdersAsync();
            }
        }

        private async void CancelOrder(object parameter)
        {
            if (parameter is SalesOrderViewModel vm)
            {
                var result = MessageBox.Show($"هل أنت متأكد من إلغاء أمر البيع رقم '{vm.SalesOrderNumber}'؟", "تأكيد الإلغاء", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;
                try
                {
                    await _salesOrderService.CancelSalesOrderAsync(vm.Id);
                    await LoadOrdersAsync();
                }
                catch (Exception ex) { MessageBox.Show($"فشلت عملية الإلغاء: {ex.Message}", "خطأ"); }
            }
        }

        private async void CreateWorkOrder(object parameter)
        {
            if (parameter is SalesOrderViewModel vm)
            {
                var result = MessageBox.Show($"سيتم البحث عن منتجات قابلة للتصنيع في أمر البيع '{vm.SalesOrderNumber}' لإنشاء أوامر عمل لها.\nهل تريد المتابعة؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.No) return;

                try
                {
                    var itemsToProcess = await _salesOrderService.GetItemsForWorkOrderCreationAsync(vm.Id);
                    int createdCount = 0;

                    foreach (var item in itemsToProcess)
                    {
                        var workOrderWindow = new AddEditWorkOrderWindow(salesOrderItemId: item.Id);
                        if (workOrderWindow.ShowDialog() == true)
                        {
                            createdCount++;
                        }
                    }

                    if (createdCount > 0)
                    {
                        await _salesOrderService.UpdateOrderStatusToInProcessAsync(vm.Id);
                        MessageBox.Show($"تم إنشاء {createdCount} أمر/أوامر عمل بنجاح.", "نجاح");
                        await LoadOrdersAsync();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ");
                }
            }
        }

        private void CreateShipment(object parameter)
        {
            if (parameter is SalesOrderViewModel vm)
            {
                var shipmentWindow = new AddShipmentWindow(vm.Id);
                if (shipmentWindow.ShowDialog() == true) LoadOrdersAsync();
            }
        }

        private void CreateInvoice(object parameter)
        {
            if (parameter is SalesOrderViewModel vm)
            {
                var invoiceWindow = new CreateInvoiceFromOrderWindow(vm.Id);
                if (invoiceWindow.ShowDialog() == true)
                {
                    LoadOrdersAsync();
                }
            }
        }
        #endregion

        #region CanExecute Logic
        private bool CanEditOrder(object p) => p is SalesOrderViewModel vm && vm.OrderStatus < OrderStatus.InProcess;
        private bool CanCancelOrder(object p) => p is SalesOrderViewModel vm && vm.OrderStatus < OrderStatus.Shipped;
        private bool CanCreateWorkOrder(object p) => p is SalesOrderViewModel vm && vm.OrderStatus != OrderStatus.Cancelled;
        private bool CanCreateShipment(object p) => p is SalesOrderViewModel vm && vm.ShippingStatus != ShippingStatus.FullyShipped && vm.OrderStatus != OrderStatus.Cancelled;
        private bool CanCreateInvoice(object p) => p is SalesOrderViewModel vm && vm.InvoicingStatus != InvoicingStatus.FullyInvoiced && vm.OrderStatus != OrderStatus.Cancelled;
        #endregion

        #region Pagination
        private async void ResetAndLoad() { if (!_isInitialized) return; _currentPage = 1; await LoadOrdersAsync(); }
        private async void GoToNextPage(object p) { if (_currentPage < GetTotalPages()) { _currentPage++; await LoadOrdersAsync(); } }
        private async void GoToPreviousPage(object p) { if (_currentPage > 1) { _currentPage--; await LoadOrdersAsync(); } }
        private int GetTotalPages() => (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / _pageSize);
        private void UpdatePageInfo() => PageInfo = $"الصفحة {_currentPage} من {GetTotalPages()} (إجمالي الأوامر: {_totalItems})";
        #endregion
    }
}