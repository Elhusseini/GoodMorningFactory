// GoodMorningFactory/UI/ViewModels/PurchasesViewModel.cs
// *** الكود الكامل والمؤكد ***
using GoodMorningFactory.Core.Helpers;
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class PurchasesViewModel : BaseViewModel
    {
        private readonly IPurchaseService _purchaseService;
        private readonly IFilterService _filterService;
        private readonly IPrintingService _printingService;

        private int _currentPage = 1;
        private readonly int _pageSize = 15;
        private int _totalItems = 0;

        private ObservableCollection<PurchaseViewModel> _purchases;
        public ObservableCollection<PurchaseViewModel> Purchases { get => _purchases; set { _purchases = value; OnPropertyChanged(); } }

        private string _searchText = "";
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ResetAndLoad(); } }

        private string _pageInfo;
        public string PageInfo { get => _pageInfo; set { _pageInfo = value; OnPropertyChanged(); } }

        public ObservableCollection<FilterItem<PurchaseInvoiceStatus?>> StatusFilters { get; set; }
        private FilterItem<PurchaseInvoiceStatus?> _selectedStatusFilter;
        public FilterItem<PurchaseInvoiceStatus?> SelectedStatusFilter { get => _selectedStatusFilter; set { _selectedStatusFilter = value; OnPropertyChanged(); ResetAndLoad(); } }

        public ICommand AddPurchaseCommand { get; }
        public ICommand EditPurchaseCommand { get; } // <-- إضافة أمر التعديل
        public ICommand RecordPaymentCommand { get; }
        public ICommand CreateDebitNoteCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        public PurchasesViewModel()
        {
            _purchaseService = new PurchaseService();
            _filterService = new FilterService();
            _printingService = new PrintingService();

            AddPurchaseCommand = new RelayCommand(AddPurchase);
            EditPurchaseCommand = new RelayCommand(EditPurchase, CanEditPurchase); // <-- تهيئة الأمر
            RecordPaymentCommand = new RelayCommand(RecordPayment, CanActOnPurchase);
            CreateDebitNoteCommand = new RelayCommand(CreateDebitNote, CanActOnPurchase);
            PrintCommand = new RelayCommand(ExecutePrint, CanActOnPurchase);
            NextPageCommand = new RelayCommand(async _ => await GoToNextPage(), _ => CanGoNext());
            PreviousPageCommand = new RelayCommand(async _ => await GoToPreviousPage(), _ => CanGoPrevious());

            Initialize();
        }

        private async void Initialize()
        {
            LoadFilters();
            await LoadPurchasesAsync();
        }

        private void LoadFilters()
        {
            var types = new ObservableCollection<FilterItem<PurchaseInvoiceStatus?>> { new FilterItem<PurchaseInvoiceStatus?> { Name = "الكل", Value = null } };
            foreach (PurchaseInvoiceStatus status in Enum.GetValues(typeof(PurchaseInvoiceStatus)))
            {
                types.Add(new FilterItem<PurchaseInvoiceStatus?> { Name = status.GetDescription(), Value = status });
            }
            StatusFilters = types;
            _selectedStatusFilter = StatusFilters.First();
            OnPropertyChanged(nameof(StatusFilters));
            OnPropertyChanged(nameof(SelectedStatusFilter));
        }

        private async Task LoadPurchasesAsync()
        {
            try
            {
                var criteria = new PurchaseFilterCriteria
                {
                    Page = _currentPage,
                    PageSize = _pageSize,
                    SearchText = this.SearchText,
                    Status = this.SelectedStatusFilter?.Value
                };
                var result = await _purchaseService.GetPurchasesAsync(criteria);
                Purchases = new ObservableCollection<PurchaseViewModel>(result.Items);
                _totalItems = result.TotalCount;
                UpdatePageInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل فواتير المشتريات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddPurchase(object parameter)
        {
            // هذا السطر صحيح ويستدعي النافذة الجديدة
            var addWindow = new AddEditPurchaseInvoiceWindow();
            if (addWindow.ShowDialog() == true) ResetAndLoad();
        }

        private void EditPurchase(object parameter)
        {
            if (parameter is PurchaseViewModel purchase)
            {
                var editWindow = new AddEditPurchaseInvoiceWindow(purchase.Id);
                if (editWindow.ShowDialog() == true) LoadPurchasesAsync();
            }
        }

        private bool CanEditPurchase(object parameter)
        {
            return parameter is PurchaseViewModel purchase && purchase.AmountPaid == 0;
        }

        private void RecordPayment(object parameter)
        {
            if (parameter is PurchaseViewModel selectedInvoice)
            {
                var paymentWindow = new RecordPurchasePaymentWindow(selectedInvoice.Id);
                if (paymentWindow.ShowDialog() == true) LoadPurchasesAsync();
            }
        }

        private void CreateDebitNote(object parameter)
        {
            if (parameter is PurchaseViewModel selectedInvoice)
            {
                var returnWindow = new AddPurchaseReturnWindow(selectedInvoice.Id);
                if (returnWindow.ShowDialog() == true) LoadPurchasesAsync();
            }
        }

        private async void ExecutePrint(object parameter)
        {
            if (parameter is PurchaseViewModel vm)
            {
                await _printingService.PrintPurchaseInvoiceAsync(vm.Id);
            }
        }

        private bool CanActOnPurchase(object parameter) => parameter is PurchaseViewModel;

        private async void ResetAndLoad()
        {
            _currentPage = 1;
            await LoadPurchasesAsync();
        }

        private async Task GoToNextPage()
        {
            if (CanGoNext()) { _currentPage++; await LoadPurchasesAsync(); }
        }
        private bool CanGoNext() => _currentPage < GetTotalPages();

        private async Task GoToPreviousPage()
        {
            if (CanGoPrevious()) { _currentPage--; await LoadPurchasesAsync(); }
        }
        private bool CanGoPrevious() => _currentPage > 1;

        private int GetTotalPages() => (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / _pageSize);

        private void UpdatePageInfo()
        {
            PageInfo = $"الصفحة {_currentPage} من {GetTotalPages()} (إجمالي الفواتير: {_totalItems})";
            (NextPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PreviousPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}