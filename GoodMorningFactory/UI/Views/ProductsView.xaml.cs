// UI/Views/ProductsView.xaml.cs
// تم تعديل هذا الملف لإضافة وظيفة التصدير

using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text; // <-- إضافة مهمة للتعامل مع النصوص
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32; // <-- إضافة مهمة لاستخدام SaveFileDialog

namespace GoodMorningFactory.UI.Views
{
    // ViewModel لعرض بيانات المنتج بشكل منسق في الواجهة
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string ProductCode { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public ProductType ProductType { get; set; }
        public string UnitOfMeasureName { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public int CurrentStock { get; set; }
        public bool IsActive { get; set; }
        public BitmapImage ProductImage { get; set; }

        // خاصية محسوبة لعرض سعر الشراء مع رمز العملة
        public string PurchasePriceFormatted => $"{PurchasePrice:N2} {AppSettings.DefaultCurrencySymbol}";

        // خاصية محسوبة لعرض سعر البيع مع رمز العملة
        public string SalePriceFormatted => $"{SalePrice:N2} {AppSettings.DefaultCurrencySymbol}";
    }

    public partial class ProductsView : UserControl
    {
        // متغيرات للتحكم في ترقيم الصفحات
        private int _currentPage = 1;
        private int _pageSize = 15;
        private int _totalItems = 0;

        public ProductsView()
        {
            InitializeComponent();
            LoadFilters(); // تحميل القوائم المنسدلة للفلاتر
            LoadProducts(); // تحميل قائمة المنتجات
        }

        // دالة لتحميل البيانات في القوائم المنسدلة الخاصة بالفلاتر
        private void LoadFilters()
        {
            using (var db = new DatabaseContext())
            {
                // إعداد فلتر الفئات
                var categories = new List<object> { new { Name = "الكل", Id = 0 } };
                categories.AddRange(db.Categories.ToList());
                CategoryFilterComboBox.ItemsSource = categories;
                CategoryFilterComboBox.SelectedIndex = 0;

                // إعداد فلتر الموردين
                var suppliers = new List<object> { new { Name = "الكل", Id = 0 } };
                suppliers.AddRange(db.Suppliers.Where(s => s.IsActive).ToList());
                SupplierFilterComboBox.ItemsSource = suppliers;
                SupplierFilterComboBox.SelectedIndex = 0;
            }

            // إعداد فلتر نوع المنتج
            var types = new List<object> { "الكل" };
            types.AddRange(Enum.GetValues(typeof(ProductType)).Cast<object>());
            TypeFilterComboBox.ItemsSource = types;
            TypeFilterComboBox.SelectedIndex = 0;

            // إعداد فلتر الحالة
            StatusFilterComboBox.ItemsSource = new List<object>
            {
                new { Name = "الكل", Value = (bool?)null },
                new { Name = "نشط", Value = (bool?)true },
                new { Name = "غير نشط", Value = (bool?)false }
            };
            StatusFilterComboBox.SelectedIndex = 0;
        }

        // دالة لجلب المنتجات من قاعدة البيانات وتطبيق الفلاتر
        private void LoadProducts()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // بناء الاستعلام الأساسي
                    var query = db.Products
                                .Include(p => p.Category)
                                .Include(p => p.UnitOfMeasure)
                                .AsQueryable();

                    // تطبيق فلتر البحث بالنص
                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(p => p.Name.ToLower().Contains(searchText) || p.ProductCode.ToLower().Contains(searchText));
                    }

                    // تطبيق فلتر الفئة
                    if (CategoryFilterComboBox.SelectedIndex > 0)
                    {
                        int categoryId = (int)CategoryFilterComboBox.SelectedValue;
                        query = query.Where(p => p.CategoryId == categoryId);
                    }

                    // تطبيق فلتر المورد
                    if (SupplierFilterComboBox.SelectedIndex > 0)
                    {
                        int supplierId = (int)SupplierFilterComboBox.SelectedValue;
                        query = query.Where(p => p.DefaultSupplierId == supplierId);
                    }

                    // تطبيق فلتر النوع
                    if (TypeFilterComboBox.SelectedItem != null && TypeFilterComboBox.SelectedItem is ProductType type)
                    {
                        query = query.Where(p => p.ProductType == type);
                    }

                    // تطبيق فلتر الحالة
                    if (StatusFilterComboBox.SelectedValue is bool status)
                    {
                        query = query.Where(p => p.IsActive == status);
                    }

                    _totalItems = query.Count();

                    // جلب بيانات الصفحة الحالية
                    var productsForPage = query.OrderBy(p => p.Name)
                                               .Skip((_currentPage - 1) * _pageSize)
                                               .Take(_pageSize)
                                               .ToList();

                    // تحويل البيانات إلى ViewModel للعرض
                    var productViewModels = new List<ProductViewModel>();
                    foreach (var p in productsForPage)
                    {
                        var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == p.Id);
                        BitmapImage image = null;
                        if (p.ProductImage != null && p.ProductImage.Length > 0)
                        {
                            image = new BitmapImage();
                            using (var stream = new MemoryStream(p.ProductImage))
                            {
                                image.BeginInit();
                                image.CacheOption = BitmapCacheOption.OnLoad;
                                image.DecodePixelWidth = 60; // تصغير حجم الصورة لتحسين الأداء
                                image.StreamSource = stream;
                                image.EndInit();
                            }
                            image.Freeze(); // تحسين الأداء في واجهات WPF
                        }

                        productViewModels.Add(new ProductViewModel
                        {
                            Id = p.Id,
                            ProductCode = p.ProductCode,
                            Name = p.Name,
                            CategoryName = p.Category?.Name,
                            ProductType = p.ProductType,
                            UnitOfMeasureName = p.UnitOfMeasure?.Name,
                            PurchasePrice = p.PurchasePrice,
                            SalePrice = p.SalePrice,
                            CurrentStock = inventory?.Quantity ?? 0,
                            IsActive = p.IsActive,
                            ProductImage = image
                        });
                    }

                    ProductsDataGrid.ItemsSource = productViewModels;
                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المنتجات: {ex.Message}\n\nInner Exception: {ex.InnerException?.Message}", "خطأ");
            }
        }

        // دالة لتحديث معلومات ترقيم الصفحات
        private void UpdatePageInfo()
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (totalPages == 0) totalPages = 1;
            PageInfoTextBlock.Text = $"الصفحة {_currentPage} من {totalPages} (إجمالي المنتجات: {_totalItems})";
            PreviousPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < totalPages;
        }

        // --- دوال معالجة أحداث الواجهة ---

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadProducts();
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (_currentPage < totalPages)
            {
                _currentPage++;
                LoadProducts();
            }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                _currentPage = 1;
                LoadProducts();
            }
        }

        private void Filter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _currentPage = 1;
                LoadProducts();
            }
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditProductWindow();
            if (addWindow.ShowDialog() == true) { _currentPage = 1; LoadProducts(); }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is ProductViewModel product)
            {
                var editWindow = new AddEditProductWindow(productId: product.Id);
                if (editWindow.ShowDialog() == true) { LoadProducts(); }
            }
        }

        private void DuplicateButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is ProductViewModel productToDuplicate)
            {
                var duplicateWindow = new AddEditProductWindow(sourceProductIdToCopy: productToDuplicate.Id);
                if (duplicateWindow.ShowDialog() == true) { _currentPage = 1; LoadProducts(); }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is ProductViewModel product)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف المنتج '{product.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DatabaseContext())
                        {
                            // التحقق من عدم وجود المنتج في عمليات مفتوحة
                            bool onOpenPO = db.PurchaseOrderItems.Any(i => i.ProductId == product.Id && i.PurchaseOrder.Status != PurchaseOrderStatus.FullyReceived && i.PurchaseOrder.Status != PurchaseOrderStatus.Cancelled);
                            if (onOpenPO)
                            {
                                MessageBox.Show("لا يمكن حذف المنتج لوجوده في أوامر شراء مفتوحة.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            bool onOpenSO = db.SalesOrderItems.Any(i => i.ProductId == product.Id && i.SalesOrder.Status != OrderStatus.Shipped && i.SalesOrder.Status != OrderStatus.Invoiced && i.SalesOrder.Status != OrderStatus.Cancelled);
                            if (onOpenSO)
                            {
                                MessageBox.Show("لا يمكن حذف المنتج لوجوده في أوامر بيع مفتوحة.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // التحقق من عدم وجود مخزون
                            var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == product.Id);
                            if (inventory != null && inventory.Quantity > 0)
                            {
                                MessageBox.Show("لا يمكن حذف المنتج لوجود مخزون حالي له.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            var productToDelete = db.Products.Find(product.Id);
                            if (productToDelete != null)
                            {
                                db.Products.Remove(productToDelete);
                                db.SaveChanges();
                                LoadProducts();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"لا يمكن حذف هذا المنتج لوجود عمليات مرتبطة به.\nالتفاصيل: {ex.InnerException?.Message ?? ex.Message}", "خطأ");
                    }
                }
            }
        }

        private void ManageUomButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.NavigateTo("UnitsOfMeasure");
        }

        private void ManagePriceListsButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.NavigateTo("PriceLists");
        }

        // === بداية التحديث: إضافة دالة زر إدارة الفئات ===
        private void ManageCategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.NavigateTo("Categories");
        }
        // === نهاية التحديث ===

        // --- بداية الإضافة: دالة زر التصدير ---
        private void ExportToCsvButton_Click(object sender, RoutedEventArgs e)
        {
            // التحقق من وجود صلاحية التصدير
            if (!PermissionsService.CanAccess("Reports.Export"))
            {
                MessageBox.Show("ليس لديك صلاحية لتصدير البيانات.", "وصول مرفوض", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // جلب البيانات المعروضة حالياً في الجدول
            var dataToExport = ProductsDataGrid.ItemsSource as IEnumerable<ProductViewModel>;
            if (dataToExport == null || !dataToExport.Any())
            {
                MessageBox.Show("لا توجد بيانات لتصديرها.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // فتح نافذة حفظ الملف
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV (Comma delimited) (*.csv)|*.csv",
                FileName = $"Products_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // استخدام StringBuilder لبناء محتوى الملف بكفاءة
                    var sb = new StringBuilder();

                    // إضافة سطر العناوين (Header)
                    sb.AppendLine("ProductCode,Name,CategoryName,ProductType,SalePrice,CurrentStock,IsActive");

                    // إضافة أسطر البيانات
                    foreach (var product in dataToExport)
                    {
                        var line = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",{4},{5},{6}",
                            product.ProductCode,
                            product.Name,
                            product.CategoryName,
                            product.ProductType,
                            product.SalePrice,
                            product.CurrentStock,
                            product.IsActive);
                        sb.AppendLine(line);
                    }

                    // كتابة المحتوى إلى الملف المحدد مع استخدام ترميز يدعم اللغة العربية
                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show("تم تصدير البيانات بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل تصدير الملف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        // --- نهاية الإضافة ---
    }
}
