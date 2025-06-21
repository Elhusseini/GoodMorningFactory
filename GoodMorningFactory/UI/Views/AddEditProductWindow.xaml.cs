// UI/Views/AddEditProductWindow.xaml.cs
// *** الكود الكامل والنهائي: تم دمج إصلاح منطق المخزون مع الكود الأصلي الشامل ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditProductWindow : Window
    {
        private int? _productId;
        private int? _sourceProductIdToCopy;
        private Product _productToEdit;
        private byte[] _productImageBytes;

        public AddEditProductWindow(int? productId = null, int? sourceProductIdToCopy = null)
        {
            InitializeComponent();
            _productId = productId;
            _sourceProductIdToCopy = sourceProductIdToCopy;
            LoadComboBoxes();

            if (_productId.HasValue)
            {
                Title = "تعديل منتج";
                LoadProductData(_productId.Value);
            }
            else if (_sourceProductIdToCopy.HasValue)
            {
                Title = "نسخ منتج جديد";
                LoadProductData(_sourceProductIdToCopy.Value, isCopy: true);
            }
            else
            {
                Title = "إضافة منتج جديد";
                _productToEdit = new Product();
                ProductCodeTextBox.Text = GenerateNewProductCode();
                IsActiveCheckBox.IsChecked = true;
                TrackInventoryCheckBox.IsChecked = true;
                SetDefaultCurrency();
            }
        }

        // دالة لتوليد كود منتج فريد
        private string GenerateNewProductCode()
        {
            return $"PROD-{DateTime.Now:yyyyMMddHHmmss}";
        }

        // دالة لتحميل البيانات في جميع القوائم المنسدلة
        private void LoadComboBoxes()
        {
            ProductTypeComboBox.ItemsSource = Enum.GetValues(typeof(ProductType));
            // --- بداية الإضافة: تحميل أنواع التتبع ---
            TrackingMethodComboBox.ItemsSource = Enum.GetValues(typeof(ProductTrackingMethod))
                .Cast<ProductTrackingMethod>()
                .Select(e => new { Value = e, Description = GetEnumDescription(e) });
            // --- نهاية الإضافة ---
            using (var db = new DatabaseContext())
            {
                CategoryComboBox.ItemsSource = db.Categories.ToList();
                UnitOfMeasureComboBox.ItemsSource = db.UnitsOfMeasure.ToList();
                DefaultSupplierComboBox.ItemsSource = db.Suppliers.Where(s => s.IsActive).ToList();
                CopyFromProductComboBox.ItemsSource = db.Products.OrderBy(p => p.Name).ToList();
                CurrencyComboBox.ItemsSource = db.Currencies.Where(c => c.IsActive).ToList();
                TaxRuleComboBox.ItemsSource = db.TaxRules.ToList();
            }
        }

        // دالة لتعيين العملة الافتراضية للنظام
        private void SetDefaultCurrency()
        {
            using (var db = new DatabaseContext())
            {
                var companyInfo = db.CompanyInfos.FirstOrDefault();
                if (companyInfo?.DefaultCurrencyId != null)
                {
                    CurrencyComboBox.SelectedValue = companyInfo.DefaultCurrencyId;
                }
            }
        }

        // دالة لتحميل بيانات منتج موجود (للتعديل أو النسخ)
        private void LoadProductData(int id, bool isCopy = false)
        {
            using (var db = new DatabaseContext())
            {
                _productToEdit = db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);
                if (_productToEdit != null)
                {
                    NameTextBox.Text = isCopy ? $"{_productToEdit.Name} (نسخة)" : _productToEdit.Name;
                    DescriptionTextBox.Text = _productToEdit.Description;
                    ProductTypeComboBox.SelectedItem = _productToEdit.ProductType;
                    CategoryComboBox.SelectedValue = _productToEdit.CategoryId;
                    UnitOfMeasureComboBox.SelectedValue = _productToEdit.UnitOfMeasureId;
                    IsActiveCheckBox.IsChecked = _productToEdit.IsActive;
                    _productImageBytes = _productToEdit.ProductImage;
                    TrackingMethodComboBox.SelectedValue = _productToEdit.TrackingMethod; // <-- تحميل القيمة

                    DisplayImage();
                    BarcodeTextBox.Text = _productToEdit.Barcode;
                    PurchasePriceTextBox.Text = _productToEdit.PurchasePrice.ToString();
                    SalePriceTextBox.Text = _productToEdit.SalePrice.ToString();
                    CurrencyComboBox.SelectedValue = _productToEdit.CurrencyId;
                    TaxRuleComboBox.SelectedValue = _productToEdit.TaxRuleId;

                    var inventory = db.Inventories.AsNoTracking().FirstOrDefault(i => i.ProductId == _productToEdit.Id);
                    if (inventory != null)
                    {
                        ReorderLevelTextBox.Text = inventory.ReorderLevel.ToString();
                        MinStockLevelTextBox.Text = inventory.MinStockLevel.ToString();
                        MaxStockLevelTextBox.Text = inventory.MaxStockLevel.ToString();
                        TrackInventoryCheckBox.IsChecked = true;
                    }
                    else
                    {
                        TrackInventoryCheckBox.IsChecked = false;
                    }
                    LeadTimeDaysTextBox.Text = _productToEdit.LeadTimeDays.ToString();
                    DefaultSupplierComboBox.SelectedValue = _productToEdit.DefaultSupplierId;

                    if (isCopy)
                    {
                        ProductCodeTextBox.Text = GenerateNewProductCode();
                        _productToEdit.Id = 0;
                        _productId = null;
                    }
                    else
                    {
                        ProductCodeTextBox.Text = _productToEdit.ProductCode;
                    }
                }
            }
        }

        // --- دوال معالجة أحداث الواجهة ---

        private void UploadImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "ملفات الصور (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg" };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _productImageBytes = File.ReadAllBytes(openFileDialog.FileName);
                    DisplayImage();
                }
                catch (Exception ex) { MessageBox.Show($"فشل تحميل الصورة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void DisplayImage()
        {
            if (_productImageBytes != null && _productImageBytes.Length > 0)
            {
                BitmapImage image = new BitmapImage();
                using (MemoryStream stream = new MemoryStream(_productImageBytes))
                {
                    image.BeginInit(); image.CacheOption = BitmapCacheOption.OnLoad; image.StreamSource = stream; image.EndInit();
                }
                ProductImage.Source = image;
            }
            else { ProductImage.Source = null; }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProductCodeTextBox.Text) ||
                string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                ProductTypeComboBox.SelectedValue == null ||
                CategoryComboBox.SelectedValue == null ||
                CurrencyComboBox.SelectedValue == null)
            {
                MessageBox.Show("يرجى ملء جميع الحقول المطلوبة (الكود، الاسم، النوع، الفئة، والعملة).", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    bool isNew = !_productId.HasValue;
                    Product productToSave;

                    if (isNew)
                    {
                        if (await db.Products.AnyAsync(p => p.ProductCode == ProductCodeTextBox.Text))
                        {
                            MessageBox.Show("كود المنتج مستخدم بالفعل. يرجى اختيار كود آخر.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        productToSave = new Product();
                        db.Products.Add(productToSave);
                    }
                    else
                    {
                        productToSave = await db.Products.FindAsync(_productId.Value);
                        if (productToSave == null) throw new Exception("لم يتم العثور على المنتج المطلوب تعديله");
                    }

                    // تعيين قيم المنتج من حقول الإدخال
                    productToSave.ProductCode = ProductCodeTextBox.Text;
                    productToSave.Name = NameTextBox.Text;
                    productToSave.Description = DescriptionTextBox.Text;
                    productToSave.ProductType = (ProductType)ProductTypeComboBox.SelectedItem;
                    productToSave.CategoryId = (int)CategoryComboBox.SelectedValue;
                    productToSave.CurrencyId = (int)CurrencyComboBox.SelectedValue;
                    productToSave.UnitOfMeasureId = (int?)UnitOfMeasureComboBox.SelectedValue;
                    productToSave.IsActive = IsActiveCheckBox.IsChecked ?? true;
                    productToSave.ProductImage = _productImageBytes;
                    productToSave.Barcode = BarcodeTextBox.Text;
                    decimal.TryParse(PurchasePriceTextBox.Text, out decimal purchasePrice);
                    productToSave.PurchasePrice = purchasePrice;
                    decimal.TryParse(SalePriceTextBox.Text, out decimal salePrice);
                    productToSave.SalePrice = salePrice;
                    productToSave.DefaultSupplierId = (int?)DefaultSupplierComboBox.SelectedValue;
                    int.TryParse(LeadTimeDaysTextBox.Text, out int leadTime);
                    productToSave.LeadTimeDays = leadTime;
                    productToSave.TaxRuleId = (int?)TaxRuleComboBox.SelectedValue;
                    // --- بداية الإضافة: حفظ طريقة التتبع ---
                    if (TrackingMethodComboBox.SelectedValue != null)
                    {
                        productToSave.TrackingMethod = (ProductTrackingMethod)TrackingMethodComboBox.SelectedValue;
                    }
                    // --- نهاية الإضافة ---

                    await db.SaveChangesAsync();

                    // إدارة سجل المخزون
                    bool trackInventory = TrackInventoryCheckBox.IsChecked ?? false;
                    var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == productToSave.Id);

                    if (trackInventory)
                    {
                        if (inventory == null)
                        {
                            var defaultLocation = await db.StorageLocations.FirstOrDefaultAsync();

                            if (defaultLocation != null)
                            {
                                inventory = new Inventory
                                {
                                    ProductId = productToSave.Id,
                                    StorageLocationId = defaultLocation.Id, // الربط بالموقع الفرعي
                                    Quantity = 0,
                                    QuantityReserved = 0
                                };
                                db.Inventories.Add(inventory);
                            }
                            else
                            {
                                MessageBox.Show("تم حفظ المنتج، لكن لا توجد أي مواقع تخزين معرفة في النظام. يرجى إضافة مخزن وموقع فرعي ثم إنشاء سجل المخزون يدوياً.", "تنبيه");
                            }
                        }

                        if (inventory != null)
                        {
                            int.TryParse(ReorderLevelTextBox.Text, out int reorderLevel);
                            inventory.ReorderLevel = reorderLevel;
                            int.TryParse(MinStockLevelTextBox.Text, out int minStock);
                            inventory.MinStockLevel = minStock;
                            int.TryParse(MaxStockLevelTextBox.Text, out int maxStock);
                            inventory.MaxStockLevel = maxStock;
                        }
                    }
                    else
                    {
                        if (inventory != null) db.Inventories.Remove(inventory);
                    }

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    MessageBox.Show($"حدث خطأ أثناء حفظ المنتج: {ex.Message}\n\nInner Exception: {ex.InnerException?.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void CopyFromProductComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;

            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Enter)
            {
                return;
            }

            string searchText = comboBox.Text;
            comboBox.IsDropDownOpen = true;

            using (var db = new DatabaseContext())
            {
                if (string.IsNullOrEmpty(searchText))
                {
                    comboBox.ItemsSource = db.Products.OrderBy(p => p.Name).ToList();
                }
                else
                {
                    var filteredProducts = db.Products
                        .Where(p => p.Name.ToLower().Contains(searchText.ToLower()) || p.ProductCode.ToLower().Contains(searchText.ToLower()))
                        .OrderBy(p => p.Name)
                        .ToList();
                    comboBox.ItemsSource = filteredProducts;
                }
            }
        }

        private void CopyDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (CopyFromProductComboBox.SelectedValue is int sourceId && sourceId > 0)
            {
                _productId = null;
                _sourceProductIdToCopy = sourceId;
                Title = "نسخ منتج جديد";
                LoadProductData(sourceId, isCopy: true);
                MessageBox.Show("تم استدعاء بيانات المنتج. يرجى مراجعة البيانات وتعديل الكود والاسم حسب الحاجة قبل الحفظ.", "تم الاستدعاء", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("يرجى اختيار منتج من القائمة لنسخ بياناته.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (System.ComponentModel.DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(System.ComponentModel.DescriptionAttribute));
            return attribute == null ? value.ToString() : attribute.Description;
        }

    }
}