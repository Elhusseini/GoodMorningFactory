// UI/Views/ProductsView.xaml.cs
// *** الكود الكامل للكود الخلفي لواجهة إدارة المنتجات مع إضافة دالة الحذف ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public partial class ProductsView : UserControl
    {
        public ProductsView()
        {
            InitializeComponent();
            LoadFilters();
            LoadProducts();
        }

        private void LoadFilters()
        {
            using (var db = new DatabaseContext())
            {
                var categories = new List<object> { new { Name = "الكل", Id = 0 } };
                categories.AddRange(db.Categories.ToList());
                CategoryFilterComboBox.ItemsSource = categories;
                CategoryFilterComboBox.SelectedIndex = 0;
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = from p in db.Products.Include(p => p.Category)
                                join i in db.Inventories on p.Id equals i.ProductId into gj
                                from subInv in gj.DefaultIfEmpty()
                                select new ProductViewModel
                                {
                                    Id = p.Id,
                                    ProductCode = p.ProductCode,
                                    Name = p.Name,
                                    CategoryName = p.Category.Name,
                                    ProductType = p.ProductType.ToString(),
                                    SalePrice = p.SalePrice,
                                    CurrentStock = subInv == null ? 0 : subInv.Quantity,
                                    IsActive = p.IsActive
                                };

                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(p => p.Name.ToLower().Contains(searchText) || p.ProductCode.ToLower().Contains(searchText));
                    }
                    if (CategoryFilterComboBox.SelectedIndex > 0)
                    {
                        int categoryId = (int)CategoryFilterComboBox.SelectedValue;
                        query = query.Where(p => db.Products.First(x => x.Id == p.Id).CategoryId == categoryId);
                    }

                    ProductsDataGrid.ItemsSource = query.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المنتجات: {ex.Message}", "خطأ");
            }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e) { if (this.IsLoaded) LoadProducts(); }
        private void Filter_KeyUp(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) LoadProducts(); }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditProductWindow();
            if (addWindow.ShowDialog() == true) { LoadProducts(); }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is ProductViewModel product)
            {
                var editWindow = new AddEditProductWindow(product.Id);
                if (editWindow.ShowDialog() == true) { LoadProducts(); }
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
                        MessageBox.Show($"لا يمكن حذف هذا المنتج لوجود عمليات مرتبطة به.\nالتفاصيل: {ex.Message}", "خطأ");
                    }
                }
            }
        }
    }
}