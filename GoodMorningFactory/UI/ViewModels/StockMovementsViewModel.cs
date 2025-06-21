using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class StockMovementsViewModel : BaseViewModel
    {
        private readonly IStockMovementService _movementService;
        private bool _isInitializing = true;

        #region Properties
        private ObservableCollection<StockMovementViewModel> _movements;
        public ObservableCollection<StockMovementViewModel> Movements
        {
            get => _movements;
            set { _movements = value; OnPropertyChanged(); }
        }

        public List<object> TypeFilters { get; private set; }
        public List<Product> ProductFilters { get; private set; }
        public List<Warehouse> WarehouseFilters { get; private set; }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ResetAndLoad(); } }

        private object _selectedType;
        public object SelectedType { get => _selectedType; set { _selectedType = value; OnPropertyChanged(); ResetAndLoad(); } }

        private Product _selectedProduct;
        public Product SelectedProduct { get => _selectedProduct; set { _selectedProduct = value; OnPropertyChanged(); ResetAndLoad(); } }

        private Warehouse _selectedWarehouse;
        public Warehouse SelectedWarehouse { get => _selectedWarehouse; set { _selectedWarehouse = value; OnPropertyChanged(); ResetAndLoad(); } }

        private DateTime? _fromDate;
        public DateTime? FromDate { get => _fromDate; set { _fromDate = value; OnPropertyChanged(); ResetAndLoad(); } }

        private DateTime? _toDate;
        public DateTime? ToDate { get => _toDate; set { _toDate = value; OnPropertyChanged(); ResetAndLoad(); } }

        private string _pageInfo;
        public string PageInfo { get => _pageInfo; set { _pageInfo = value; OnPropertyChanged(); } }
        private int _currentPage = 1;
        private readonly int _pageSize = 25;
        private int _totalItems = 0;
        #endregion

        #region Commands
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        #endregion

        public StockMovementsViewModel()
        {
            _movementService = new StockMovementService();
            NextPageCommand = new RelayCommand(GoToNextPage);
            PreviousPageCommand = new RelayCommand(GoToPreviousPage);
            Initialize();
        }

        private async void Initialize()
        {
            await LoadFiltersAsync();
            await LoadMovementsAsync();
            _isInitializing = false;
        }

        private async Task LoadFiltersAsync()
        {
            var filters = await _movementService.GetStockMovementFiltersAsync();
            TypeFilters = filters.MovementTypes;
            _selectedType = TypeFilters.FirstOrDefault();

            var allProducts = new List<Product> { new Product { Name = "الكل", Id = 0 } };
            allProducts.AddRange(filters.Products);
            ProductFilters = allProducts;
            _selectedProduct = ProductFilters.FirstOrDefault();

            var allWarehouses = new List<Warehouse> { new Warehouse { Name = "الكل", Id = 0 } };
            allWarehouses.AddRange(filters.Warehouses);
            WarehouseFilters = allWarehouses;
            _selectedWarehouse = WarehouseFilters.FirstOrDefault();

            OnPropertyChanged(nameof(TypeFilters));
            OnPropertyChanged(nameof(ProductFilters));
            OnPropertyChanged(nameof(WarehouseFilters));
        }

        private async Task LoadMovementsAsync()
        {
            var criteria = new StockMovementFilterCriteria
            {
                Page = _currentPage,
                PageSize = _pageSize,
                SearchText = this.SearchText,
                MovementType = (SelectedType is StockMovementType type) ? type : (StockMovementType?)null,
                ProductId = SelectedProduct?.Id == 0 ? null : SelectedProduct?.Id,
                WarehouseId = SelectedWarehouse?.Id == 0 ? null : SelectedWarehouse?.Id,
                FromDate = this.FromDate,
                ToDate = this.ToDate
            };

            var result = await _movementService.GetStockMovementsAsync(criteria);
            Movements = new ObservableCollection<StockMovementViewModel>(result.Items);
            _totalItems = result.TotalCount;
            UpdatePageInfo();
        }

        #region Helpers
        private async void ResetAndLoad()
        {
            if (_isInitializing) return;
            _currentPage = 1;
            await LoadMovementsAsync();
        }
        private async void GoToNextPage(object p) { if (_currentPage < GetTotalPages()) { _currentPage++; await LoadMovementsAsync(); } }
        private async void GoToPreviousPage(object p) { if (_currentPage > 1) { _currentPage--; await LoadMovementsAsync(); } }
        private int GetTotalPages() => (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / _pageSize);
        private void UpdatePageInfo() => PageInfo = $"الصفحة {_currentPage} من {GetTotalPages()} (إجمالي السجلات: {_totalItems})";
        #endregion
    }
}