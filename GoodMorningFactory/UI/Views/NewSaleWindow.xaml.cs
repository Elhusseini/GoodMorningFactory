// UI/Views/NewSaleWindow.xaml.cs
// *** تم تصحيح طريقة حذف المنتج من الفاتورة ***

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
    // ViewModel لعرض بيانات بنود الفاتورة في الواجهة
    public class SaleItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }
        public decimal Subtotal => UnitPrice * Quantity;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class NewSaleWindow : Window
    {
        private ObservableCollection<SaleItemViewModel> _invoiceItems = new ObservableCollection<SaleItemViewModel>();

        public NewSaleWindow()
        {
            InitializeComponent();
            InvoiceItemsDataGrid.ItemsSource = _invoiceItems;
            _invoiceItems.CollectionChanged += (s, e) => UpdateTotal();
        }

        private void SearchProductTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                using (var db = new DatabaseContext())
                {
                    var searchText = SearchProductTextBox.Text.ToLower();
                    var product = db.Products.FirstOrDefault(p => p.ProductCode.ToLower() == searchText || p.Name.ToLower().Contains(searchText));

                    if (product != null)
                    {
                        AddProductToInvoice(product);
                        SearchProductTextBox.Clear();
                    }
                    else
                    {
                        MessageBox.Show("لم يتم العثور على المنتج.", "بحث", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void AddProductToInvoice(Product product)
        {
            var existingItem = _invoiceItems.FirstOrDefault(item => item.ProductId == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                _invoiceItems.Add(new SaleItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.SalePrice,
                    Quantity = 1
                });
            }
        }

        // *** بداية التصحيح ***
        // تم تغيير طريقة الحذف بالكامل لتكون أكثر أماناً
        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            // الطريقة الأكثر أماناً:
            // المرسل (sender) هو الزر الذي تم النقر عليه.
            // الـ DataContext الخاص به هو الـ ViewModel للصف الذي يتواجد فيه.
            if ((sender as FrameworkElement)?.DataContext is SaleItemViewModel itemToRemove)
            {
                // نقوم بإزالة العنصر مباشرة من القائمة.
                _invoiceItems.Remove(itemToRemove);
            }
        }
        // *** نهاية التصحيح ***

        private void UpdateTotal()
        {
            decimal total = _invoiceItems.Sum(item => item.Subtotal);
            TotalAmountTextBlock.Text = $"{total:C}";
        }

        private void CompleteSaleButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_invoiceItems.Any())
            {
                MessageBox.Show("الفاتورة فارغة. يرجى إضافة منتجات أولاً.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(AmountPaidTextBox.Text, out decimal amountPaid) || amountPaid < 0)
            {
                MessageBox.Show("يرجى إدخال مبلغ مدفوع صحيح.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var sale = new Sale
                    {
                        InvoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}",
                        SaleDate = DateTime.Now,
                        TotalAmount = _invoiceItems.Sum(item => item.Subtotal),
                        AmountPaid = amountPaid
                    };
                    db.Sales.Add(sale);
                    db.SaveChanges();

                    foreach (var item in _invoiceItems)
                    {
                        var saleItem = new SaleItem
                        {
                            SaleId = sale.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        };
                        db.SaleItems.Add(saleItem);

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

                    db.SaveChanges();
                    transaction.Commit();

                    MessageBox.Show("تمت عملية البيع بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشلت عملية البيع: {ex.Message}", "خطأ فادح", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
