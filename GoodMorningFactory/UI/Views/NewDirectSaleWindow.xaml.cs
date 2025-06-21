// UI/Views/NewDirectSaleWindow.xaml.cs
// *** تحديث: حفظ هوية العميل مباشرة مع الفاتورة ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public class DirectSaleItemViewModel : INotifyPropertyChanged
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

    public partial class NewDirectSaleWindow : Window
    {
        private ObservableCollection<DirectSaleItemViewModel> _items = new ObservableCollection<DirectSaleItemViewModel>();

        public NewDirectSaleWindow()
        {
            InitializeComponent();
            AppSettings.LoadSettings(); // <-- إضافة هذا السطر
            SaleDatePicker.SelectedDate = DateTime.Today;
            SaleItemsDataGrid.ItemsSource = _items;
            _items.CollectionChanged += (s, e) => UpdateTotal();
            using (var db = new DatabaseContext())
            {
                CustomerComboBox.ItemsSource = db.Customers.Where(c => c.IsActive).ToList();
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
                        _items.Add(new DirectSaleItemViewModel
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
        // تعديل دالة UpdateTotal():
        private void UpdateTotal()
        {
            string currencySymbol = AppSettings.DefaultCurrencySymbol;
            TotalAmountTextBlock.Text = $"{_items.Sum(i => i.Subtotal):N2} {currencySymbol}";
        }
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is DirectSaleItemViewModel item) _items.Remove(item);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomerComboBox.SelectedItem == null || !_items.Any())
            {
                MessageBox.Show("يرجى اختيار العميل وإضافة منتجات.", "بيانات ناقصة");
                return;
            }

            decimal.TryParse(AmountPaidTextBox.Text, out decimal amountPaid);

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // --- بداية التحديث: إضافة CustomerId ---
                    var newSale = new Sale
                    {
                        InvoiceNumber = $"INV-POS-{DateTime.Now:yyyyMMddHHmmss}",
                        SaleDate = SaleDatePicker.SelectedDate ?? DateTime.Today,
                        CustomerId = (int)CustomerComboBox.SelectedValue, // <-- إضافة مهمة
                        Status = InvoiceStatus.Sent,
                        TotalAmount = _items.Sum(i => i.Subtotal),
                        AmountPaid = amountPaid
                    };
                    // --- نهاية التحديث ---


                    foreach (var item in _items)
                    {
                        newSale.SaleItems.Add(new SaleItem { ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = item.UnitPrice });
                        var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == item.ProductId);
                        if (inventory != null && inventory.Quantity >= item.Quantity)
                        {
                            inventory.Quantity -= item.Quantity;
                        }
                        else
                        {
                            throw new Exception($"الكمية غير كافية للمنتج: {item.ProductName}");
                        }
                    }

                    db.Sales.Add(newSale);
                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم حفظ الفاتورة بنجاح.");
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