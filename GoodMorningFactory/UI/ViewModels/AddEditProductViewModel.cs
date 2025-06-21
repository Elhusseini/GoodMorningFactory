// GoodMorningFactory/UI/ViewModels/AddEditProductViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private Product _product;
        private byte[] _productImageBytes;
        private bool _isCopyMode = false;

        #region Properties
        public string Title { get; set; }
        public string ProductCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ProductType ProductType { get; set; }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged();
                    if (_product.Id == 0)
                    {
                        GenerateCodeForCategory();
                    }
                }
            }
        }

        public string Barcode { get; set; }
        public bool IsActive { get; set; }
        public BitmapImage ProductImage { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public Currency SelectedCurrency { get; set; }
        public TaxRule SelectedTaxRule { get; set; }

        // --- بداية التعديل: تعديل خاصية تتبع المخزون ---
        private bool _trackInventory;
        public bool TrackInventory
        {
            get => _trackInventory;
            set
            {
                if (_trackInventory != value)
                {
                    _trackInventory = value;
                    OnPropertyChanged();
                    // إشعار الواجهة بتغيير حالة الإظهار
                    OnPropertyChanged(nameof(InventoryFieldsVisibility));
                }
            }
        }

        // --- خاصية جديدة للتحكم في إظهار الحقول بناءً على الخاصية أعلاه ---
        public Visibility InventoryFieldsVisibility => TrackInventory ? Visibility.Visible : Visibility.Collapsed;
        // --- نهاية التعديل ---

        public UnitOfMeasure SelectedUnitOfMeasure { get; set; }
        public int ReorderLevel { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }
        public Supplier SelectedDefaultSupplier { get; set; }
        public int LeadTimeDays { get; set; }
        public ProductTrackingMethod TrackingMethod { get; set; }
        public List<Category> Categories { get; set; }
        public List<UnitOfMeasure> UnitsOfMeasure { get; set; }
        public List<Supplier> Suppliers { get; set; }
        public List<Currency> Currencies { get; set; }
        public List<TaxRule> TaxRules { get; set; }
        public List<StorageLocation> StorageLocations { get; set; }
        public StorageLocation SelectedPrimaryLocation { get; set; }
        public List<Product> SearchableProducts { get; set; }
        public Product SelectedProductToCopy { get; set; }
        public string CopySearchText { get; set; }

        public IEnumerable<ProductType> ProductTypes => Enum.GetValues(typeof(ProductType)).Cast<ProductType>();
        public IEnumerable<ProductTrackingMethod> TrackingMethods => Enum.GetValues(typeof(ProductTrackingMethod)).Cast<ProductTrackingMethod>();
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        public ICommand UploadImageCommand { get; }
        public ICommand CopyDataCommand { get; }
        #endregion

        // Constructor للتصميم (Design-Time)
        public AddEditProductViewModel()
        {
            Title = "إضافة / تعديل منتج";
        }

        public AddEditProductViewModel(int? productId = null, int? sourceProductIdToCopy = null)
        {
            _productService = new ProductService();
            SaveCommand = new RelayCommand(Save, CanSave);
            UploadImageCommand = new RelayCommand(UploadImage);
            CopyDataCommand = new RelayCommand(CopyData);
            _isCopyMode = sourceProductIdToCopy.HasValue;
            LoadInitialData(productId, sourceProductIdToCopy);
        }

        private async void GenerateCodeForCategory()
        {
            if (SelectedCategory != null && SelectedCategory.Id != 0)
            {
                ProductCode = await _productService.GenerateNextProductCodeAsync(SelectedCategory.Id);
                OnPropertyChanged(nameof(ProductCode));
            }
            else
            {
                ProductCode = "اختر فئة لتوليد الكود";
                OnPropertyChanged(nameof(ProductCode));
            }
        }

        private async Task LoadInitialData(int? productId, int? sourceProductIdToCopy)
        {
            try
            {
                var dto = await _productService.GetInitialDataForAddEditWindowAsync(productId, sourceProductIdToCopy);

                Categories = dto.Categories;
                UnitsOfMeasure = dto.UnitsOfMeasure;
                Suppliers = dto.Suppliers;
                Currencies = dto.Currencies;
                TaxRules = dto.TaxRules;
                StorageLocations = dto.StorageLocations;

                PopulateViewModelFromModel(dto.Product, dto.Inventory, sourceProductIdToCopy.HasValue);

                if (dto.Product.Id == 0 && !sourceProductIdToCopy.HasValue)
                {
                    SelectedCurrency = Currencies.FirstOrDefault(c => c.Id == dto.DefaultCurrencyId);
                }

                OnPropertyChanged(string.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        private void PopulateViewModelFromModel(Product product, Inventory inventory, bool isCopy)
        {
            _product = product;
            if (isCopy) { _product.Id = 0; }

            Title = isCopy ? "نسخ منتج جديد" : (product.Id > 0 ? "تعديل منتج" : "إضافة منتج جديد");
            ProductCode = product.ProductCode;
            Name = isCopy ? $"{product.Name} (نسخة)" : product.Name;
            Description = product.Description;
            ProductType = product.ProductType;
            SelectedCategory = Categories?.FirstOrDefault(c => c.Id == product.CategoryId);
            Barcode = product.Barcode;
            IsActive = product.IsActive;
            _productImageBytes = product.ProductImage;
            DisplayImage();
            PurchasePrice = product.PurchasePrice;
            SalePrice = product.SalePrice;
            SelectedCurrency = Currencies?.FirstOrDefault(c => c.Id == product.CurrencyId);
            SelectedTaxRule = TaxRules?.FirstOrDefault(t => t.Id == product.TaxRuleId);
            TrackInventory = product.TrackInventory; // سيقوم هذا السطر بتحديث خاصية الإظهار تلقائياً
            if (inventory != null)
            {
                SelectedPrimaryLocation = StorageLocations?.FirstOrDefault(sl => sl.Id == inventory.StorageLocationId);
                ReorderLevel = inventory.ReorderLevel;
                MinStockLevel = inventory.MinStockLevel;
                MaxStockLevel = inventory.MaxStockLevel;
            }
            else
            {
                ReorderLevel = 0; MinStockLevel = 0; MaxStockLevel = 0;
                SelectedPrimaryLocation = StorageLocations?.FirstOrDefault(sl => sl.IsDefault);
            }
            SelectedUnitOfMeasure = UnitsOfMeasure?.FirstOrDefault(u => u.Id == product.UnitOfMeasureId);
            SelectedDefaultSupplier = Suppliers?.FirstOrDefault(s => s.Id == product.DefaultSupplierId);
            LeadTimeDays = product.LeadTimeDays;
            TrackingMethod = product.TrackingMethod;
        }

        private void UploadImage(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Image Files|*.png;*.jpeg;*.jpg" };
            if (openFileDialog.ShowDialog() == true)
            {
                _productImageBytes = File.ReadAllBytes(openFileDialog.FileName);
                DisplayImage();
            }
        }

        private void DisplayImage()
        {
            if (_productImageBytes != null && _productImageBytes.Length > 0)
            {
                BitmapImage image = new BitmapImage();
                using (var stream = new MemoryStream(_productImageBytes))
                {
                    image.BeginInit(); image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream; image.EndInit();
                }
                ProductImage = image;
                OnPropertyChanged(nameof(ProductImage));
            }
        }

        private async void CopyData(object obj)
        {
            if (SelectedProductToCopy != null)
            {
                await LoadInitialData(null, SelectedProductToCopy.Id);
            }
        }

        private async void Save(object parameter)
        {
            _product.ProductCode = ProductCode;
            _product.Name = Name;
            _product.Description = Description;
            _product.ProductType = ProductType;
            _product.CategoryId = SelectedCategory.Id;
            _product.CurrencyId = SelectedCurrency.Id;
            _product.UnitOfMeasureId = SelectedUnitOfMeasure?.Id;
            _product.IsActive = IsActive;
            _product.ProductImage = _productImageBytes;
            _product.Barcode = Barcode;
            _product.PurchasePrice = PurchasePrice;
            _product.SalePrice = SalePrice;
            _product.DefaultSupplierId = SelectedDefaultSupplier?.Id;
            _product.LeadTimeDays = LeadTimeDays;
            _product.TaxRuleId = SelectedTaxRule?.Id;
            _product.TrackingMethod = TrackingMethod;
            _product.TrackInventory = TrackInventory;

            try
            {
                await _productService.SaveProductAsync(_product, TrackInventory, SelectedPrimaryLocation?.Id, ReorderLevel, MinStockLevel, MaxStockLevel);
                MessageBox.Show("تم حفظ المنتج بنجاح.");
                (parameter as Window).DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ المنتج: {ex.Message}", "خطأ");
            }
        }

        private bool CanSave(object parameter)
        {
            return !string.IsNullOrWhiteSpace(ProductCode) &&
                   !string.IsNullOrWhiteSpace(Name) &&
                   SelectedCategory != null &&
                   SelectedCategory.Id != 0 &&
                   SelectedCurrency != null;
        }
    }
}