// UI/Views/EditSaleWindow.xaml.cs
// *** الكود الكامل لنافذة تعديل الفاتورة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public class EditSaleItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => UnitPrice * Quantity;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class EditSaleWindow : Window
    {
        private readonly int _saleId;
        private ObservableCollection<EditSaleItemViewModel> _items = new ObservableCollection<EditSaleItemViewModel>();

        public EditSaleWindow(int saleId)
        {
            InitializeComponent();
            _saleId = saleId;
            SaleItemsDataGrid.ItemsSource = _items;
            _items.CollectionChanged += (s, e) => UpdateTotal();
            LoadSaleData();
        }

        private void LoadSaleData()
        {
            using (var db = new DatabaseContext())
            {
                var sale = db.Sales
                             .Include(s => s.SalesOrder)
                             .Include(s => s.SaleItems).ThenInclude(si => si.Product)
                             .FirstOrDefault(s => s.Id == _saleId);

                if (sale == null)
                {
                    MessageBox.Show("لم يتم العثور على الفاتورة.");
                    this.Close();
                    return;
                }

                CustomerComboBox.ItemsSource = db.Customers.ToList();

                CustomerComboBox.SelectedValue = sale.SalesOrder?.CustomerId;
                SaleDatePicker.SelectedDate = sale.SaleDate;
                AmountPaidTextBox.Text = sale.AmountPaid.ToString("N2");

                foreach (var item in sale.SaleItems)
                {
                    _items.Add(new EditSaleItemViewModel
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });
                }
            }
        }

        private void SearchProductTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            using (var db = new DatabaseContext())
            {
                var product = db.Products.FirstOrDefault(p => p.ProductCode.ToLower() == SearchProductTextBox.Text.ToLower() || p.Name.ToLower().Contains(SearchProductTextBox.Text.ToLower()));
                if (product != null)
                {
                    var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);
                    if (existingItem != null) { existingItem.Quantity++; }
                    else
                    {
                        _items.Add(new EditSaleItemViewModel
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            Quantity = 1,
                            UnitPrice = product.SalePrice
                        });
                    }
                    SearchProductTextBox.Clear();
                }
            }
        }

        private void UpdateTotal() => TotalAmountTextBlock.Text = $"{_items.Sum(i => i.Subtotal):C}";
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is EditSaleItemViewModel item) _items.Remove(item);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_items.Any())
            {
                MessageBox.Show("لا يمكن حفظ فاتورة فارغة.", "بيانات ناقصة");
                return;
            }

            decimal.TryParse(AmountPaidTextBox.Text, out decimal amountPaid);

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var saleToUpdate = db.Sales.Include(s => s.SaleItems).FirstOrDefault(s => s.Id == _saleId);
                    if (saleToUpdate == null) return;

                    foreach (var oldItem in saleToUpdate.SaleItems)
                    {
                        var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == oldItem.ProductId);
                        if (inventory != null) { inventory.Quantity += oldItem.Quantity; }
                    }
                    db.SaleItems.RemoveRange(saleToUpdate.SaleItems);

                    saleToUpdate.SaleDate = SaleDatePicker.SelectedDate ?? DateTime.Today;
                    saleToUpdate.TotalAmount = _items.Sum(i => i.Subtotal);
                    saleToUpdate.AmountPaid = amountPaid;

                    foreach (var newItem in _items)
                    {
                        saleToUpdate.SaleItems.Add(new SaleItem { ProductId = newItem.ProductId, Quantity = newItem.Quantity, UnitPrice = newItem.UnitPrice });
                        var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == newItem.ProductId);
                        if (inventory != null && inventory.Quantity >= newItem.Quantity)
                        {
                            inventory.Quantity -= newItem.Quantity;
                        }
                        else
                        {
                            throw new Exception($"الكمية غير كافية للمنتج: {newItem.ProductName}");
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم حفظ تعديلات الفاتورة بنجاح.");
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشلت العملية: {ex.Message}");
                }
            }
        }
    }
}