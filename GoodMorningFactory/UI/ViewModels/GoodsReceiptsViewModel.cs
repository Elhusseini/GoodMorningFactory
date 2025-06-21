// GoodMorningFactory/UI/ViewModels/GoodsReceiptsViewModel.cs
// *** الكود الكامل والنهائي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class GoodsReceiptsViewModel : BaseViewModel
    {
        // --- بداية الإضافة: إضافة خدمة الطباعة ---
        private readonly IGoodsReceiptService _goodsReceiptService;
        private readonly IPrintingService _printingService;
        // --- نهاية الإضافة ---

        private int _currentPage = 1;
        private const int PageSize = 15;
        private int _totalItems = 0;

        private ObservableCollection<GoodsReceiptNote> _goodsReceipts;
        public ObservableCollection<GoodsReceiptNote> GoodsReceipts { get => _goodsReceipts; set { _goodsReceipts = value; OnPropertyChanged(); } }

        private string _pageInfo;
        public string PageInfo { get => _pageInfo; set { _pageInfo = value; OnPropertyChanged(); } }

        public RelayCommand ViewDetailsCommand { get; }
        public RelayCommand CreateInvoiceCommand { get; }
        public RelayCommand PrintCommand { get; }
        public RelayCommand NextPageCommand { get; }
        public RelayCommand PreviousPageCommand { get; }
        public RelayCommand RefreshCommand { get; }

        public GoodsReceiptsViewModel()
        {
            _goodsReceiptService = new GoodsReceiptService();
            _printingService = new PrintingService(); // <-- تهيئة خدمة الطباعة

            ViewDetailsCommand = new RelayCommand(ViewDetails);
            CreateInvoiceCommand = new RelayCommand(CreateInvoice, CanCreateInvoice);

            // --- بداية الإصلاح: ربط أمر الطباعة بالدالة الصحيحة ---
            PrintCommand = new RelayCommand(Print, CanPrint);
            // --- نهاية الإصلاح ---

            NextPageCommand = new RelayCommand(async _ => await NextPage(), _ => CanGoNext());
            PreviousPageCommand = new RelayCommand(async _ => await PreviousPage(), _ => CanGoPrevious());
            RefreshCommand = new RelayCommand(async _ => await LoadGoodsReceipts());

            LoadGoodsReceipts();
        }

        private async Task LoadGoodsReceipts()
        {
            try
            {
                var result = await _goodsReceiptService.GetPagedGoodsReceiptsAsync(_currentPage, PageSize);
                _totalItems = result.TotalItems;
                GoodsReceipts = new ObservableCollection<GoodsReceiptNote>(result.Items);
                UpdatePageInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل سندات الاستلام: {ex.Message}", "خطأ");
            }
        }

        private void UpdatePageInfo()
        {
            int totalPages = (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / PageSize);
            PageInfo = $"الصفحة {_currentPage} من {totalPages} (إجمالي السجلات: {_totalItems})";
            NextPageCommand.RaiseCanExecuteChanged();
            PreviousPageCommand.RaiseCanExecuteChanged();
        }

        private void ViewDetails(object parameter)
        {
            if (parameter is GoodsReceiptNote grn)
            {
                var detailsWindow = new GoodsReceiptDetailWindow(grn.Id);
                detailsWindow.ShowDialog();
            }
        }

        private void CreateInvoice(object parameter)
        {
            if (parameter is GoodsReceiptNote grn)
            {
                var invoiceWindow = new AddEditPurchaseInvoiceWindow(grnId: grn.Id);
                if (invoiceWindow.ShowDialog() == true)
                {
                    LoadGoodsReceipts();
                }
            }
        }

        // ======================= بداية الإصلاح =======================
        // الآن يتم التحقق من الخاصية المحسوبة الجديدة
        private bool CanCreateInvoice(object parameter) => parameter is GoodsReceiptNote grn && !grn.IsInvoiced;
        // ======================== نهاية الإصلاح ========================
    

private async void Print(object parameter)
        {
            if (parameter is GoodsReceiptNote grn)
            {
                await _printingService.PrintGoodsReceiptNoteAsync(grn.Id);
            }
        }
        private bool CanPrint(object parameter) => parameter is GoodsReceiptNote;
        // --- نهاية الإصلاح ---

        private async Task NextPage() { _currentPage++; await LoadGoodsReceipts(); }
        private bool CanGoNext() => _currentPage < (int)Math.Ceiling((double)_totalItems / PageSize);

        private async Task PreviousPage() { _currentPage--; await LoadGoodsReceipts(); }
        private bool CanGoPrevious() => _currentPage > 1;
    }
}