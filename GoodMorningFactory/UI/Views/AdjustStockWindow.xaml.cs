// UI/Views/AdjustStockWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة تعديل المخزون ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public partial class AdjustStockWindow : Window
    {
        private ObservableCollection<StockAdjustmentItemViewModel> _itemsToAdjust = new ObservableCollection<StockAdjustmentItemViewModel>();

        public AdjustStockWindow()
        {
            InitializeComponent();
            AdjustmentDatePicker.SelectedDate = DateTime.Today;
            ItemsDataGrid.ItemsSource = _itemsToAdjust;
        }

        private void SearchProductTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            string searchText = SearchProductTextBox.Text;
            if (string.IsNullOrWhiteSpace(searchText)) return;

            using (var db = new DatabaseContext())
            {
                var product = db.Products.FirstOrDefault(p => p.ProductCode.ToLower() == searchText.ToLower() || p.Name.ToLower().Contains(searchText.ToLower()));

                if (product != null)
                {
                    if (!_itemsToAdjust.Any(i => i.ProductId == product.Id))
                    {
                        var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == product.Id);
                        int systemQty = inventory?.Quantity ?? 0;

                        _itemsToAdjust.Add(new StockAdjustmentItemViewModel
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            SystemQuantity = systemQty,
                            ActualQuantity = systemQty
                        });
                    }
                    SearchProductTextBox.Clear();
                }
                else
                {
                    MessageBox.Show("لم يتم العثور على المنتج.", "بحث", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void PostAdjustmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_itemsToAdjust.Any())
            {
                MessageBox.Show("يرجى إضافة منتجات لتعديل كمياتها.", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in _itemsToAdjust)
                    {
                        if (item.Difference != 0)
                        {
                            var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == item.ProductId);
                            if (inventory != null)
                            {
                                inventory.Quantity = item.ActualQuantity;
                            }
                            else
                            {
                                db.Inventories.Add(new Inventory { ProductId = item.ProductId, Quantity = item.ActualQuantity });
                            }
                        }
                    }
                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم ترحيل تعديلات المخزون بنجاح.", "نجاح");
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ");
                }
            }
        }
    }
}