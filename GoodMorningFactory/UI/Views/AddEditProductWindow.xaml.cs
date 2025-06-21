// UI/Views/AddEditProductWindow.xaml.cs
// *** الكود الكامل للكود الخلفي لنافذة إضافة وتعديل منتج مع الإصلاحات ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditProductWindow : Window
    {
        private int? _productId;
        private Product _productToEdit;

        public AddEditProductWindow(int? productId = null)
        {
            InitializeComponent();
            _productId = productId;
            LoadComboBoxes();
            if (_productId.HasValue)
            {
                LoadProductData();
            }
        }

        private void LoadComboBoxes()
        {
            ProductTypeComboBox.ItemsSource = Enum.GetValues(typeof(ProductType));
            using (var db = new DatabaseContext())
            {
                CategoryComboBox.ItemsSource = db.Categories.ToList();
                UnitOfMeasureComboBox.ItemsSource = db.UnitsOfMeasure.ToList();
            }
        }

        private void LoadProductData()
        {
            using (var db = new DatabaseContext())
            {
                _productToEdit = db.Products.Find(_productId.Value);
                if (_productToEdit != null)
                {
                    ProductCodeTextBox.Text = _productToEdit.ProductCode;
                    NameTextBox.Text = _productToEdit.Name;
                    ProductTypeComboBox.SelectedItem = _productToEdit.ProductType;
                    CategoryComboBox.SelectedValue = _productToEdit.CategoryId;
                    UnitOfMeasureComboBox.SelectedValue = _productToEdit.UnitOfMeasureId;
                    IsActiveCheckBox.IsChecked = _productToEdit.IsActive;
                    PurchasePriceTextBox.Text = _productToEdit.PurchasePrice.ToString();
                    SalePriceTextBox.Text = _productToEdit.SalePrice.ToString();

                    var inventory = db.Inventories
                        .FirstOrDefault(i => i.ProductId == _productToEdit.Id);
                    if (inventory != null)
                    {
                        ReorderLevelTextBox.Text = inventory.ReorderLevel.ToString();
                    }
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProductCodeTextBox.Text) || 
                string.IsNullOrWhiteSpace(NameTextBox.Text) || 
                ProductTypeComboBox.SelectedItem == null || 
                CategoryComboBox.SelectedItem == null ||
                CategoryComboBox.SelectedValue == null)
            {
                MessageBox.Show("يرجى ملء جميع الحقول المطلوبة.", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    Product product;
                    bool isNew = !_productId.HasValue;

                    if (isNew)
                    {
                        product = new Product();
                        db.Products.Add(product);
                    }
                    else
                    {
                        product = db.Products.Find(_productId.Value);
                        if (product == null)
                        {
                            throw new Exception("لم يتم العثور على المنتج المطلوب تعديله");
                        }
                    }

                    // Update product properties
                    product.ProductCode = ProductCodeTextBox.Text;
                    product.Name = NameTextBox.Text;
                    product.ProductType = (ProductType)ProductTypeComboBox.SelectedItem;
                    product.CategoryId = (int)CategoryComboBox.SelectedValue;
                    product.UnitOfMeasureId = (int?)UnitOfMeasureComboBox.SelectedValue;
                    product.IsActive = IsActiveCheckBox.IsChecked ?? true;

                    if (decimal.TryParse(PurchasePriceTextBox.Text, out decimal purchasePrice))
                    {
                        product.PurchasePrice = purchasePrice;
                    }

                    if (decimal.TryParse(SalePriceTextBox.Text, out decimal salePrice))
                    {
                        product.SalePrice = salePrice;
                    }

                    // Save product first
                    db.SaveChanges();

                    // Now handle inventory
                    // Get the main warehouse if new product
                    var defaultWarehouse = db.Warehouses
                        .FirstOrDefault(w => w.Code == "WH-MAIN" || w.IsActive);

                    if (defaultWarehouse == null)
                    {
                        throw new Exception("لم يتم العثور على مخزن افتراضي نشط");
                    }

                    var inventory = await db.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == product.Id && i.WarehouseId == defaultWarehouse.Id);

                    if (inventory == null)
                    {
                        inventory = new Inventory
                        {
                            ProductId = product.Id,
                            WarehouseId = defaultWarehouse.Id,
                            Quantity = 0,
                            QuantityReserved = 0
                        };
                        db.Inventories.Add(inventory);
                    }

                    if (int.TryParse(ReorderLevelTextBox.Text, out int reorderLevel))
                    {
                        inventory.ReorderLevel = reorderLevel;
                    }

                    await db.SaveChangesAsync();
                    transaction.Commit();

                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"حدث خطأ أثناء حفظ المنتج: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}