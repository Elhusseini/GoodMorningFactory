// GoodMorningFactory/UI/ViewModels/InventoryStatusViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands; // تأكد من أن هذا المسار صحيح لمشروعك
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
    /// <summary>
    /// ViewModel يدير منطق عرض شاشة "أرصدة المخزون" بالهيكل الجديد (رئيسي-تفصيلي).
    /// </summary>
    public class InventoryStatusViewModel : BaseViewModel
    {
        private readonly IInventoryService _inventoryService;
        private bool _isInitialized = false;

        #region الخصائص (Properties)

        // خصائص الفلاتر والبحث
        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ResetAndLoad(); } }

        public ObservableCollection<Warehouse> WarehouseFilters { get; private set; }
        private Warehouse _selectedWarehouse;
        public Warehouse SelectedWarehouse { get => _selectedWarehouse; set { _selectedWarehouse = value; OnPropertyChanged(); ResetAndLoad(); } }

        public ObservableCollection<Category> CategoryFilters { get; private set; }
        private Category _selectedCategory;
        public Category SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); ResetAndLoad(); } }

        public ObservableCollection<object> StatusFilters { get; private set; }
        private object _selectedStatus;
        public object SelectedStatus { get => _selectedStatus; set { _selectedStatus = value; OnPropertyChanged(); ResetAndLoad(); } }

        // --- بداية التعديل الجوهري ---

        // القائمة الرئيسية للمنتجات المجمعة
        private ObservableCollection<GroupedInventoryViewModel> _inventoryItems;
        public ObservableCollection<GroupedInventoryViewModel> InventoryItems
        {
            get => _inventoryItems;
            set { _inventoryItems = value; OnPropertyChanged(); }
        }

        // المنتج المختار حاليًا من القائمة الرئيسية لعرض تفاصيله
        private GroupedInventoryViewModel _selectedInventoryItem;
        public GroupedInventoryViewModel SelectedInventoryItem
        {
            get => _selectedInventoryItem;
            set { _selectedInventoryItem = value; OnPropertyChanged(); }
        }
        // --- نهاية التعديل الجوهري ---


        // خصائص ترقيم الصفحات
        private string _pageInfo;
        public string PageInfo { get => _pageInfo; set { _pageInfo = value; OnPropertyChanged(); } }
        private int _currentPage = 1;
        private readonly int _pageSize = 15; // يمكنك تعديل حجم الصفحة حسب الرغبة
        private int _totalItems = 0;

        #endregion

        #region الأوامر (Commands)
        public ICommand LoadDataCommand { get; }
        public ICommand QuickAdjustCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        // يمكنك إضافة أوامر أخرى هنا بنفس الطريقة
        // public ICommand ViewHistoryCommand { get; }
        // public ICommand ViewPurchaseHistoryCommand { get; }
        #endregion

        public InventoryStatusViewModel()
        {
            _inventoryService = new InventoryService();

            InventoryItems = new ObservableCollection<GroupedInventoryViewModel>();
            WarehouseFilters = new ObservableCollection<Warehouse>();
            CategoryFilters = new ObservableCollection<Category>();
            StatusFilters = new ObservableCollection<object>();

            LoadDataCommand = new RelayCommand(async _ => await InitializeAsync());
            NextPageCommand = new RelayCommand(GoToNextPage, _ => _currentPage < GetTotalPages());
            PreviousPageCommand = new RelayCommand(GoToPreviousPage, _ => _currentPage > 1);

            // لاحظ أن أمر التعديل السريع الآن يجب أن يعمل مع كائن من نوع InventoryLocationDetailViewModel
            QuickAdjustCommand = new RelayCommand(ExecuteQuickAdjust, param => param is InventoryLocationDetailViewModel);
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            await LoadFiltersAsync();
            await LoadInventoryAsync();
        }

        private async Task LoadFiltersAsync()
        {
            var filters = await _inventoryService.GetInventoryFiltersAsync();

            WarehouseFilters.Clear();
            WarehouseFilters.Add(new Warehouse { Id = 0, Name = "الكل" });
            filters.Warehouses.ForEach(w => WarehouseFilters.Add(w));
            _selectedWarehouse = WarehouseFilters.FirstOrDefault();

            CategoryFilters.Clear();
            CategoryFilters.Add(new Category { Id = 0, Name = "الكل" });
            filters.Categories.ForEach(c => CategoryFilters.Add(c));
            _selectedCategory = CategoryFilters.FirstOrDefault();

            StatusFilters.Clear();
            filters.Statuses.ForEach(s => StatusFilters.Add(s));
            _selectedStatus = StatusFilters.FirstOrDefault();

            // إشعار الواجهة بتحديث جميع الفلاتر مرة واحدة
            OnPropertyChanged(nameof(WarehouseFilters));
            OnPropertyChanged(nameof(SelectedWarehouse));
            OnPropertyChanged(nameof(CategoryFilters));
            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(StatusFilters));
            OnPropertyChanged(nameof(SelectedStatus));
        }

        private async Task LoadInventoryAsync()
        {
            if (_inventoryService == null) return;

            var criteria = new InventoryFilterCriteria
            {
                Page = _currentPage,
                PageSize = _pageSize,
                SearchText = this.SearchText,
                WarehouseId = _selectedWarehouse?.Id == 0 ? (int?)null : _selectedWarehouse?.Id,
                CategoryId = _selectedCategory?.Id == 0 ? (int?)null : _selectedCategory?.Id,
                Status = _selectedStatus is StockStatus status ? status : (StockStatus?)null
            };

            var result = await _inventoryService.GetInventoryStatusAsync(criteria);

            InventoryItems.Clear();
            if (result?.Items != null)
            {
                foreach (var item in result.Items) { InventoryItems.Add(item); }
            }

            _totalItems = result.TotalCount;
            UpdatePageInfo();

            // تحديد أول عنصر في القائمة الرئيسية تلقائيًا لعرض تفاصيله
            SelectedInventoryItem = InventoryItems.FirstOrDefault();
        }

        private void ExecuteQuickAdjust(object parameter)
        {
            // الأمر الآن يستقبل تفاصيل الموقع المحدد
            if (parameter is InventoryLocationDetailViewModel item)
            {
                var adjustWindow = new QuickStockAdjustWindow(item.ProductId, item.StorageLocationId);
                if (adjustWindow.ShowDialog() == true)
                {
                    // إعادة تحميل البيانات لتحديث الواجهة بعد التعديل
                    LoadInventoryAsync();
                }
            }
        }

        #region مساعدات ترقيم الصفحات والتحميل
        private async void ResetAndLoad()
        {
            if (!_isInitialized) return;
            _currentPage = 1;
            await LoadInventoryAsync();
        }

        private async void GoToNextPage(object p)
        {
            _currentPage++;
            await LoadInventoryAsync();
        }

        private async void GoToPreviousPage(object p)
        {
            _currentPage--;
            await LoadInventoryAsync();
        }

        private int GetTotalPages() => (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / _pageSize);

        private void UpdatePageInfo() => PageInfo = $"الصفحة {_currentPage} من {GetTotalPages()} (الإجمالي: {_totalItems} منتج)";
        #endregion
    }
}