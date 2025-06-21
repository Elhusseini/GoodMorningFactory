// GoodMorningFactory/UI/ViewModels/PurchaseOrdersViewModel.cs
// *** الكود الكامل والنهائي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class PurchaseOrdersViewModel : BaseViewModel
    {
        private readonly IPurchaseOrderService _poService;
        private readonly IPrintingService _printingService;

        private int _currentPage = 1;
        private const int PageSize = 15;
        private int _totalItems = 0;

        private ObservableCollection<PurchaseOrderViewModel> _orders;
        public ObservableCollection<PurchaseOrderViewModel> Orders { get => _orders; set { _orders = value; OnPropertyChanged(); } }

        public List<object> StatusFilters { get; private set; }
        private object _selectedStatusFilter;
        public object SelectedStatusFilter { get => _selectedStatusFilter; set { _selectedStatusFilter = value; OnPropertyChanged(); ResetAndLoad(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }

        private string _pageInfo;
        public string PageInfo { get => _pageInfo; set { _pageInfo = value; OnPropertyChanged(); } }

        public RelayCommand AddPurchaseOrderCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand PrintCommand { get; }
        public RelayCommand ReceiveGoodsCommand { get; }
        public RelayCommand CreateInvoiceCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand NextPageCommand { get; }
        public RelayCommand PreviousPageCommand { get; }
        public RelayCommand RefreshCommand { get; }

        public PurchaseOrdersViewModel()
        {
            _poService = new PurchaseOrderService();
            _printingService = new PrintingService();

            AddPurchaseOrderCommand = new RelayCommand(AddPurchaseOrder);
            EditCommand = new RelayCommand(EditOrder, CanEditOrCancel);
            CancelCommand = new RelayCommand(CancelOrder, CanEditOrCancel);
            PrintCommand = new RelayCommand(PrintOrder);
            ReceiveGoodsCommand = new RelayCommand(ReceiveGoods, CanReceive);
            CreateInvoiceCommand = new RelayCommand(CreateInvoice, CanInvoice);
            SearchCommand = new RelayCommand(param => ResetAndLoad());
            NextPageCommand = new RelayCommand(async _ => await NextPage(), _ => CanGoNext());
            PreviousPageCommand = new RelayCommand(async _ => await PreviousPage(), _ => CanGoPrevious());
            RefreshCommand = new RelayCommand(async _ => await LoadPurchaseOrders());

            LoadStatusFilters();
            LoadPurchaseOrders();
        }

        private void LoadStatusFilters()
        {
            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(PurchaseOrderStatus)).Cast<object>());
            StatusFilters = statuses;
            SelectedStatusFilter = StatusFilters.First();
        }

        public async Task LoadPurchaseOrders()
        {
            try
            {
                PurchaseOrderStatus? status = null;
                if (SelectedStatusFilter is PurchaseOrderStatus selectedStatus)
                {
                    status = selectedStatus;
                }

                var result = await _poService.GetPurchaseOrdersAsync(_currentPage, PageSize, SearchText, status);
                _totalItems = result.TotalItems;
                var viewModels = result.Items.Select(order => new PurchaseOrderViewModel { Order = order });
                Orders = new ObservableCollection<PurchaseOrderViewModel>(viewModels);
                UpdatePageInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل أوامر الشراء: {ex.Message}", "خطأ");
            }
        }

        private void CreateInvoice(object obj)
        {
            if (obj is PurchaseOrderViewModel vm)
            {
                // ======================= بداية الإصلاح =======================
                // التحقق من وجود أي سندات استلام غير مفوترة لهذا الأمر
                using (var db = new Data.DatabaseContext())
                {
                    // نستخدم الآن PurchaseId للتحقق
                    bool hasUninvoicedReceipts = db.GoodsReceiptNotes.Any(grn => grn.PurchaseOrderId == vm.Order.Id && grn.PurchaseId == null);
                    if (!hasUninvoicedReceipts)
                    {
                        MessageBox.Show("لا توجد بضاعة مستلمة وغير مفوترة لإنشاء فاتورة لها.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                // ======================== نهاية الإصلاح ========================

                var invoiceWindow = new AddEditPurchaseInvoiceWindow(purchaseOrderId: vm.Order.Id);
                if (invoiceWindow.ShowDialog() == true) LoadPurchaseOrders();
            }
        }

        private void UpdatePageInfo()
        {
            int totalPages = (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / PageSize);
            PageInfo = $"الصفحة {_currentPage} من {totalPages} (إجمالي السجلات: {_totalItems})";
            NextPageCommand.RaiseCanExecuteChanged();
            PreviousPageCommand.RaiseCanExecuteChanged();
        }

        private void ResetAndLoad()
        {
            _currentPage = 1;
            LoadPurchaseOrders();
        }

        private void AddPurchaseOrder(object obj)
        {
            var addWindow = new AddEditPurchaseOrderWindow();
            if (addWindow.ShowDialog() == true) LoadPurchaseOrders();
        }

        private void EditOrder(object obj)
        {
            if (obj is PurchaseOrderViewModel vm)
            {
                var editWindow = new AddEditPurchaseOrderWindow(poId: vm.Order.Id);
                if (editWindow.ShowDialog() == true) LoadPurchaseOrders();
            }
        }

        private async void CancelOrder(object obj)
        {
            if (obj is PurchaseOrderViewModel vm)
            {
                await _poService.CancelPurchaseOrderAsync(vm.Order.Id);
                await LoadPurchaseOrders();
            }
        }

        private async void PrintOrder(object obj)
        {
            if (obj is PurchaseOrderViewModel vm)
            {
                await _printingService.PrintPurchaseOrderAsync(vm.Order.Id);
            }
        }

        private void ReceiveGoods(object obj)
        {
            if (obj is PurchaseOrderViewModel vm)
            {
                var receiveWindow = new AddGoodsReceiptWindow(vm.Order.Id);
                if (receiveWindow.ShowDialog() == true) LoadPurchaseOrders();
            }
        }
        // ======================= بداية الإصلاح الرئيسي =======================
        // تحسين منطق تفعيل وتعطيل الأزرار
        private bool CanEditOrCancel(object obj) => obj is PurchaseOrderViewModel vm && vm.ReceiptStatus == ReceiptStatus.NotReceived && vm.InvoicingStatus == POInvoicingStatus.NotInvoiced && vm.Status != PurchaseOrderStatus.Cancelled;
        private bool CanReceive(object obj) => obj is PurchaseOrderViewModel vm && vm.ReceiptStatus != ReceiptStatus.FullyReceived && vm.Status != PurchaseOrderStatus.Cancelled;
        private bool CanInvoice(object obj) => obj is PurchaseOrderViewModel vm && vm.InvoicingStatus != POInvoicingStatus.FullyInvoiced && vm.ReceiptStatus != ReceiptStatus.NotReceived && vm.Status != PurchaseOrderStatus.Cancelled;
        // ======================== نهاية الإصلاح الرئيسي ========================


        private async Task NextPage() { _currentPage++; await LoadPurchaseOrders(); }
        private bool CanGoNext() => _currentPage < (int)Math.Ceiling((double)_totalItems / PageSize);

        private async Task PreviousPage() { _currentPage--; await LoadPurchaseOrders(); }
        private bool CanGoPrevious() => _currentPage > 1;
    }
}