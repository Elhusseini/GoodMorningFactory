// GoodMorningFactory/UI/ViewModels/SuppliersViewViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class SuppliersViewViewModel : BaseViewModel
    {
        private readonly ISupplierService _supplierService;
        private readonly IFilterService _filterService;
        private int _currentPage = 1;
        private readonly int _pageSize = 15;
        private int _totalItems = 0;

        #region Properties
        private ObservableCollection<SupplierViewModel> _suppliers;
        public ObservableCollection<SupplierViewModel> Suppliers { get => _suppliers; set { _suppliers = value; OnPropertyChanged(); } }

        private string _searchText = "";
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ResetAndLoad(); } }

        private string _pageInfo;
        public string PageInfo { get => _pageInfo; set { _pageInfo = value; OnPropertyChanged(); } }

        public ObservableCollection<FilterItem<bool?>> StatusFilters { get; set; }
        private FilterItem<bool?> _selectedStatusFilter;
        public FilterItem<bool?> SelectedStatusFilter { get => _selectedStatusFilter; set { _selectedStatusFilter = value; OnPropertyChanged(); ResetAndLoad(); } }
        #endregion

        #region Commands
        public ICommand AddSupplierCommand { get; }
        public ICommand EditSupplierCommand { get; }
        public ICommand DeleteSupplierCommand { get; }
        public ICommand ViewStatementCommand { get; }
        public ICommand ExportToCsvCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        #endregion

        public SuppliersViewViewModel()
        {
            _supplierService = new SupplierService();
            _filterService = new FilterService();

            AddSupplierCommand = new RelayCommand(AddSupplier);
            EditSupplierCommand = new RelayCommand(EditSupplier, CanActOnSupplier);
            DeleteSupplierCommand = new RelayCommand(DeleteSupplier, CanActOnSupplier);
            ViewStatementCommand = new RelayCommand(ViewStatement, CanActOnSupplier);
            ExportToCsvCommand = new RelayCommand(ExportToCsv);
            NextPageCommand = new RelayCommand(GoToNextPage, _ => _currentPage < GetTotalPages());
            PreviousPageCommand = new RelayCommand(GoToPreviousPage, _ => _currentPage > 1);

            Initialize();
        }

        private async void Initialize()
        {
            LoadFilters();
            await LoadSuppliersAsync();
        }

        private void LoadFilters()
        {
            StatusFilters = new ObservableCollection<FilterItem<bool?>>(_filterService.GetStatusFilters());
            _selectedStatusFilter = StatusFilters.First();
            OnPropertyChanged(nameof(StatusFilters));
            OnPropertyChanged(nameof(SelectedStatusFilter));
        }

        private async Task LoadSuppliersAsync()
        {
            try
            {
                var criteria = new SupplierFilterCriteria
                {
                    Page = _currentPage,
                    PageSize = _pageSize,
                    SearchText = this.SearchText,
                    IsActive = this.SelectedStatusFilter?.Value
                };
                var result = await _supplierService.GetSuppliersAsync(criteria);
                Suppliers = new ObservableCollection<SupplierViewModel>(result.Items);
                _totalItems = result.TotalCount;
                UpdatePageInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الموردين: {ex.Message}", "خطأ");
            }
        }

        private void AddSupplier(object parameter)
        {
            var addWindow = new AddEditSupplierWindow();
            if (addWindow.ShowDialog() == true)
            {
                ResetAndLoad();
            }
        }

        private void EditSupplier(object parameter)
        {
            if (parameter is SupplierViewModel supplierVM)
            {
                var editWindow = new AddEditSupplierWindow(supplierVM.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadSuppliersAsync();
                }
            }
        }

        private async void DeleteSupplier(object parameter)
        {
            if (parameter is SupplierViewModel supplierVM &&
                MessageBox.Show($"هل أنت متأكد من حذف المورد '{supplierVM.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    await _supplierService.DeleteSupplierAsync(supplierVM.Id);
                    await LoadSuppliersAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "خطأ في الحذف");
                }
            }
        }

        private void ViewStatement(object parameter)
        {
            if (parameter is SupplierViewModel supplierVM)
            {
                var statementWindow = new SupplierStatementWindow(supplierVM.Id);
                statementWindow.Show();
            }
        }

        private void ExportToCsv(object parameter)
        {
            if (Suppliers == null || !Suppliers.Any())
            {
                MessageBox.Show("لا توجد بيانات لتصديرها.", "تنبيه");
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV (Comma delimited) (*.csv)|*.csv",
                FileName = $"Suppliers_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("SupplierCode,Name,ContactPerson,PhoneNumber,CurrentBalance,IsActive");
                    foreach (var supplier in Suppliers)
                    {
                        var line = $"\"{supplier.SupplierCode}\",\"{supplier.Name}\",\"{supplier.ContactPerson}\",\"{supplier.PhoneNumber}\",{supplier.CurrentBalance},{supplier.IsActive}";
                        sb.AppendLine(line);
                    }
                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("تم تصدير البيانات بنجاح.", "نجاح");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل تصدير الملف: {ex.Message}", "خطأ");
                }
            }
        }
        private bool CanActOnSupplier(object parameter) => parameter is SupplierViewModel;

        #region Pagination Helpers
        private async void ResetAndLoad() { _currentPage = 1; await LoadSuppliersAsync(); }
        private void GoToNextPage(object parameter) { if (_currentPage < GetTotalPages()) { _currentPage++; LoadSuppliersAsync(); } }
        private void GoToPreviousPage(object parameter) { if (_currentPage > 1) { _currentPage--; LoadSuppliersAsync(); } }
        private int GetTotalPages() => (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / _pageSize);
        private void UpdatePageInfo() { PageInfo = $"الصفحة {_currentPage} من {GetTotalPages()} (إجمالي الموردين: {_totalItems})"; }
        #endregion
    }
}
