// GoodMorningFactory/UI/ViewModels/InventoryCountsViewModel.cs
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
using System.Collections.Generic; // لإضافة List

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel لإدارة واجهة عرض قائمة أوامر الجرد.
    /// </summary>
    public class InventoryCountsViewModel : BaseViewModel
    {
        private readonly IInventoryCountService _inventoryCountService;

        #region الخصائص (Properties)
        private ObservableCollection<InventoryCountViewModel> _counts;
        public ObservableCollection<InventoryCountViewModel> Counts
        {
            get => _counts;
            set { _counts = value; OnPropertyChanged(); }
        }

        public List<object> StatusFilters { get; private set; }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ResetAndLoad(); } }

        private object _selectedStatus;
        public object SelectedStatus { get => _selectedStatus; set { _selectedStatus = value; OnPropertyChanged(); ResetAndLoad(); } }

        private string _pageInfo;
        public string PageInfo { get => _pageInfo; set { _pageInfo = value; OnPropertyChanged(); } }
        private int _currentPage = 1;
        private readonly int _pageSize = 20;
        private int _totalItems = 0;
        #endregion

        #region الأوامر (Commands)
        public ICommand AddCountCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        #endregion

        public InventoryCountsViewModel()
        {
            _inventoryCountService = new InventoryCountService();
            Counts = new ObservableCollection<InventoryCountViewModel>();

            AddCountCommand = new RelayCommand(ExecuteAddCount);
            ViewDetailsCommand = new RelayCommand(ExecuteViewDetails);
            CancelCommand = new RelayCommand(ExecuteCancel, CanExecuteCancel);
            RefreshCommand = new RelayCommand(async _ => await LoadCountsAsync());
            NextPageCommand = new RelayCommand(GoToNextPage, _ => _currentPage < GetTotalPages());
            PreviousPageCommand = new RelayCommand(GoToPreviousPage, _ => _currentPage > 1);

            Initialize();
        }

        private async void Initialize()
        {
            LoadFilters();
            await LoadCountsAsync();
        }

        private void LoadFilters()
        {
            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(InventoryCountStatus)).Cast<object>());
            StatusFilters = statuses;
            SelectedStatus = StatusFilters.First();
            OnPropertyChanged(nameof(StatusFilters));
        }

        private async Task LoadCountsAsync()
        {
            try
            {
                var criteria = new InventoryCountFilterCriteria
                {
                    Page = _currentPage,
                    PageSize = _pageSize,
                    SearchText = this.SearchText,
                    Status = (SelectedStatus is InventoryCountStatus status) ? status : (InventoryCountStatus?)null
                };

                var result = await _inventoryCountService.GetInventoryCountsAsync(criteria);

                var viewModels = result.Items.Select(ic => new InventoryCountViewModel
                {
                    Id = ic.Id,
                    CountReferenceNumber = ic.CountReferenceNumber,
                    CountDate = ic.CountDate,
                    WarehouseName = ic.Warehouse.Name,
                    Status = ic.Status,
                    ResponsibleUser = ic.ResponsibleUser?.Username ?? "غير محدد"
                }).ToList();

                Counts = new ObservableCollection<InventoryCountViewModel>(viewModels);
                _totalItems = result.TotalCount;
                UpdatePageInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل أوامر الجرد: {ex.Message}", "خطأ");
            }
        }

        private void ExecuteAddCount(object parameter)
        {
            var addWindow = new AddEditInventoryCountWindow();
            if (addWindow.ShowDialog() == true)
            {
                ResetAndLoad();
            }
        }

        private void ExecuteViewDetails(object parameter)
        {
            if (parameter is InventoryCountViewModel vm)
            {
                var editWindow = new AddEditInventoryCountWindow(vm.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadCountsAsync();
                }
            }
        }

        private async void ExecuteCancel(object parameter)
        {
            if (parameter is InventoryCountViewModel vm)
            {
                var result = MessageBox.Show($"هل أنت متأكد من إلغاء أمر الجرد رقم '{vm.CountReferenceNumber}'؟", "تأكيد الإلغاء", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _inventoryCountService.CancelInventoryCountAsync(vm.Id);
                        await LoadCountsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"فشل إلغاء أمر الجرد: {ex.Message}", "خطأ");
                    }
                }
            }
        }

        private bool CanExecuteCancel(object parameter)
        {
            // يمكن الإلغاء فقط إذا لم يكن الأمر مكتملاً أو ملغياً بالفعل
            return parameter is InventoryCountViewModel vm &&
                   vm.Status != InventoryCountStatus.Completed &&
                   vm.Status != InventoryCountStatus.Cancelled;
        }

        #region Pagination Helpers
        private async void ResetAndLoad() { _currentPage = 1; await LoadCountsAsync(); }
        private async void GoToNextPage(object p) { if (_currentPage < GetTotalPages()) { _currentPage++; await LoadCountsAsync(); } }
        private async void GoToPreviousPage(object p) { if (_currentPage > 1) { _currentPage--; await LoadCountsAsync(); } }
        private int GetTotalPages() => (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / _pageSize);
        private void UpdatePageInfo() => PageInfo = $"الصفحة {_currentPage} من {GetTotalPages()} (إجمالي السجلات: {_totalItems})";
        #endregion
    }
}