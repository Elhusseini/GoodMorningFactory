// GoodMorningFactory/UI/ViewModels/ProductsViewViewModel.cs
using GoodMorningFactory.Core.Helpers;
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ProductsViewViewModel : BaseViewModel
    {
        #region الخدمات
        private readonly IProductService _productService;
        private readonly IFilterService _filterService;
        #endregion

        #region الحقول الخاصة بالترقيم
        private int _currentPage = 1;
        private readonly int _pageSize = 15;
        private int _totalItems = 0;
        #endregion

        #region الخصائص العامة (Properties)

        private ObservableCollection<ProductViewModel> _products;
        public ObservableCollection<ProductViewModel> Products
        {
            get => _products;
            set { _products = value; OnPropertyChanged(); }
        }

        private ProductViewModel _selectedProduct;
        public ProductViewModel SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ResetAndLoad(); }
        }

        private string _pageInfo;
        public string PageInfo
        {
            get => _pageInfo;
            set { _pageInfo = value; OnPropertyChanged(); }
        }

        public ObservableCollection<FilterItem<int>> CategoryFilters { get; set; }
        public ObservableCollection<FilterItem<int>> SupplierFilters { get; set; }
        public ObservableCollection<FilterItem<ProductType?>> ProductTypeFilters { get; set; }
        public ObservableCollection<FilterItem<bool?>> StatusFilters { get; set; }

        private FilterItem<int> _selectedCategory;
        public FilterItem<int> SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); ResetAndLoad(); }
        }

        private FilterItem<int> _selectedSupplier;
        public FilterItem<int> SelectedSupplier
        {
            get => _selectedSupplier;
            set { _selectedSupplier = value; OnPropertyChanged(); ResetAndLoad(); }
        }

        private FilterItem<ProductType?> _selectedProductType;
        public FilterItem<ProductType?> SelectedProductType
        {
            get => _selectedProductType;
            set { _selectedProductType = value; OnPropertyChanged(); ResetAndLoad(); }
        }

        private FilterItem<bool?> _selectedStatus;
        public FilterItem<bool?> SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); ResetAndLoad(); }
        }
        #endregion

        #region الأوامر (Commands)
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand DuplicateProductCommand { get; }
        public ICommand ExportToCsvCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand ManageStockCommand { get; }
        public ICommand ViewPurchaseHistoryCommand { get; }
        #endregion

        public ProductsViewViewModel()
        {
            _productService = new ProductService();
            _filterService = new FilterService();

            AddProductCommand = new RelayCommand(AddProduct);
            EditProductCommand = new RelayCommand(EditProduct, CanActOnProduct);
            DeleteProductCommand = new RelayCommand(DeleteProduct, CanActOnProduct);
            DuplicateProductCommand = new RelayCommand(DuplicateProduct, CanActOnProduct);
            ExportToCsvCommand = new RelayCommand(ExportToCsv);
            NextPageCommand = new RelayCommand(GoToNextPage, _ => _currentPage < GetTotalPages());
            PreviousPageCommand = new RelayCommand(GoToPreviousPage, _ => _currentPage > 1);

            // ربط الأوامر بالدوال المنفذة
            ManageStockCommand = new RelayCommand(ManageStock, CanActOnProduct);
            ViewPurchaseHistoryCommand = new RelayCommand(ViewPurchaseHistory, CanActOnProduct);

            Initialize();
        }

        private async void Initialize()
        {
            await LoadFiltersAsync();
            await LoadProductsAsync();
        }

        #region دوال تنفيذ الأوامر والمنطق
        private async Task LoadFiltersAsync()
        {
            CategoryFilters = new ObservableCollection<FilterItem<int>>(await _filterService.GetCategoryFiltersAsync());
            _selectedCategory = CategoryFilters.First();

            SupplierFilters = new ObservableCollection<FilterItem<int>>(await _filterService.GetSupplierFiltersAsync());
            _selectedSupplier = SupplierFilters.First();

            ProductTypeFilters = new ObservableCollection<FilterItem<ProductType?>>(_filterService.GetProductTypeFilters());
            _selectedProductType = ProductTypeFilters.First();

            StatusFilters = new ObservableCollection<FilterItem<bool?>>(_filterService.GetStatusFilters());
            _selectedStatus = StatusFilters.First();

            OnPropertyChanged(nameof(CategoryFilters));
            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(SupplierFilters));
            OnPropertyChanged(nameof(SelectedSupplier));
            OnPropertyChanged(nameof(ProductTypeFilters));
            OnPropertyChanged(nameof(SelectedProductType));
            OnPropertyChanged(nameof(StatusFilters));
            OnPropertyChanged(nameof(SelectedStatus));
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var criteria = new ProductFilterCriteria
                {
                    Page = _currentPage,
                    PageSize = _pageSize,
                    SearchText = this.SearchText,
                    CategoryId = SelectedCategory?.Value ?? 0,
                    SupplierId = SelectedSupplier?.Value ?? 0,
                    ProductType = SelectedProductType?.Value,
                    IsActive = SelectedStatus?.Value
                };

                var result = await _productService.GetProductsAsync(criteria);

                Products = new ObservableCollection<ProductViewModel>(result.Items);
                _totalItems = result.TotalCount;
                UpdatePageInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المنتجات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProduct(object parameter)
        {
            var addWindow = new AddEditProductWindow();
            if (addWindow.ShowDialog() == true) ResetAndLoad();
        }

        private void EditProduct(object parameter)
        {
            if (parameter is ProductViewModel product)
            {
                var editWindow = new AddEditProductWindow(productId: product.Id);
                if (editWindow.ShowDialog() == true) LoadProductsAsync();
            }
        }

        private void DuplicateProduct(object parameter)
        {
            if (parameter is ProductViewModel product)
            {
                var duplicateWindow = new AddEditProductWindow(sourceProductIdToCopy: product.Id);
                if (duplicateWindow.ShowDialog() == true) ResetAndLoad();
            }
        }

        private async void DeleteProduct(object parameter)
        {
            if (parameter is ProductViewModel product &&
              MessageBox.Show($"هل أنت متأكد من حذف المنتج '{product.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    await _productService.DeleteProductAsync(product.Id);
                    await LoadProductsAsync();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "عملية مرفوضة", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"حدث خطأ غير متوقع أثناء الحذف: {ex.Message}", "خطأ");
                }
            }
        }

        private void ExportToCsv(object parameter)
        {
            if (Products == null || !Products.Any())
            {
                MessageBox.Show("لا توجد بيانات لتصديرها.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV (Comma delimited) (*.csv)|*.csv",
                FileName = $"Products_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("ProductCode,Name,CategoryName,ProductType,SalePrice,PurchasePrice,CurrentStock,IsActive");

                    foreach (var product in Products)
                    {
                        var line = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",{4},{5},{6},{7}",
                          product.ProductCode, product.Name, product.CategoryName,
                          product.ProductType.GetDescription(), product.SalePrice, product.PurchasePrice,
                          product.CurrentStock, product.IsActive);
                        sb.AppendLine(line);
                    }

                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("تم تصدير البيانات بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل تصدير الملف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // --- بداية الكود المنفذ للأزرار الجديدة ---
        private void ManageStock(object parameter)
        {
            if (parameter is ProductViewModel product)
            {
                var stockWindow = new ManageProductStockWindow(product);
                if (stockWindow.ShowDialog() == true)
                {
                    // إعادة تحميل البيانات لتحديث كمية المخزون في القائمة الرئيسية
                    LoadProductsAsync();
                }
            }
        }

        private void ViewPurchaseHistory(object parameter)
        {
            if (parameter is ProductViewModel product)
            {
                var historyWindow = new ProductPurchaseHistoryWindow(product.Id);
                historyWindow.Show();
            }
        }
        // --- نهاية الكود المنفذ ---

        private bool CanActOnProduct(object parameter) => parameter is ProductViewModel;
        #endregion

        #region دوال مساعدة للترقيم
        private async void ResetAndLoad()
        {
            _currentPage = 1;
            await LoadProductsAsync();
        }

        private async void GoToNextPage(object parameter)
        {
            if (_currentPage < GetTotalPages())
            {
                _currentPage++;
                await LoadProductsAsync();
            }
        }

        private async void GoToPreviousPage(object parameter)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadProductsAsync();
            }
        }

        private int GetTotalPages() => (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / _pageSize);

        private void UpdatePageInfo()
        {
            PageInfo = $"الصفحة {_currentPage} من {GetTotalPages()} (إجمالي المنتجات: {_totalItems})";
        }
        #endregion
    }
}