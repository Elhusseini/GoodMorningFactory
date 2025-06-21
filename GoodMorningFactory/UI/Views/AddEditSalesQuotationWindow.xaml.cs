// UI/Views/AddEditSalesQuotationWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة إنشاء وتعديل عرض السعر ***
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
    public class SalesQuotationItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }

        private decimal _unitPrice;
        public decimal UnitPrice { get => _unitPrice; set { _unitPrice = value; OnPropertyChanged(nameof(UnitPrice)); OnPropertyChanged(nameof(Subtotal)); } }

        private int _quantity;
        public int Quantity { get => _quantity; set { _quantity = value; OnPropertyChanged(nameof(Quantity)); OnPropertyChanged(nameof(Subtotal)); } }

        private decimal _discount;
        public decimal Discount { get => _discount; set { _discount = value; OnPropertyChanged(nameof(Discount)); OnPropertyChanged(nameof(Subtotal)); } }

        public decimal Subtotal => (UnitPrice * Quantity) - Discount;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public partial class AddEditSalesQuotationWindow : Window
    {
        private int? _quotationId;
        private ObservableCollection<SalesQuotationItemViewModel> _items = new ObservableCollection<SalesQuotationItemViewModel>();

        public AddEditSalesQuotationWindow(int? quotationId = null)
        {
            InitializeComponent();
            _quotationId = quotationId;
            QuotationItemsDataGrid.ItemsSource = _items;
            _items.CollectionChanged += (s, e) => UpdateTotal();
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            using (var db = new DatabaseContext())
            {
                CustomerComboBox.ItemsSource = db.Customers.Where(c => c.IsActive).ToList();
            }

            if (_quotationId.HasValue) // وضع التعديل
            {
                Title = "تعديل عرض سعر";
                using (var db = new DatabaseContext())
                {
                    var quotation = db.SalesQuotations.Include(q => q.SalesQuotationItems)
                                       .ThenInclude(i => i.Product)
                                       .FirstOrDefault(q => q.Id == _quotationId.Value);
                    if (quotation != null)
                    {
                        QuotationNumberTextBox.Text = quotation.QuotationNumber;
                        CustomerComboBox.SelectedValue = quotation.CustomerId;
                        QuotationDatePicker.SelectedDate = quotation.QuotationDate;
                        ValidUntilDatePicker.SelectedDate = quotation.ValidUntilDate;

                        foreach (var item in quotation.SalesQuotationItems)
                        {
                            _items.Add(new SalesQuotationItemViewModel
                            {
                                ProductId = item.ProductId,
                                ProductName = item.Product.Name,
                                Description = item.Description,
                                UnitPrice = item.UnitPrice,
                                Quantity = item.Quantity,
                                Discount = item.Discount
                            });
                        }
                    }
                }
            }
            else // وضع الإضافة
            {
                Title = "إنشاء عرض سعر جديد";
                QuotationDatePicker.SelectedDate = DateTime.Today;
                ValidUntilDatePicker.SelectedDate = DateTime.Today.AddDays(30);
                QuotationNumberTextBox.Text = $"QUO-{DateTime.Now:yyyyMMddHHmmss}";
            }
        }

        private void SearchProductTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                using (var db = new DatabaseContext())
                {
                    var product = db.Products.FirstOrDefault(p => p.ProductCode.ToLower() == SearchProductTextBox.Text.ToLower() || p.Name.ToLower().Contains(SearchProductTextBox.Text.ToLower()));
                    if (product != null)
                    {
                        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);
                        if (existingItem != null)
                        {
                            existingItem.Quantity++;
                        }
                        else
                        {
                            _items.Add(new SalesQuotationItemViewModel
                            {
                                ProductId = product.Id,
                                ProductName = product.Name,
                                Description = product.Description,
                                Quantity = 1,
                                UnitPrice = product.SalePrice,
                                Discount = 0
                            });
                        }
                        SearchProductTextBox.Clear();
                    }
                }
            }
        }

        private void UpdateTotal() => TotalAmountTextBlock.Text = $"{_items.Sum(i => i.Subtotal):C}";
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is SalesQuotationItemViewModel item) _items.Remove(item);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomerComboBox.SelectedItem == null)
            {
                MessageBox.Show("يرجى اختيار العميل.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DatabaseContext())
            {
                SalesQuotation quotation;
                if (_quotationId.HasValue) // تعديل
                {
                    quotation = db.SalesQuotations.Include(q => q.SalesQuotationItems).FirstOrDefault(q => q.Id == _quotationId.Value);
                    db.SalesQuotationItems.RemoveRange(quotation.SalesQuotationItems);
                }
                else // إضافة
                {
                    quotation = new SalesQuotation();
                    db.SalesQuotations.Add(quotation);
                }

                quotation.QuotationNumber = QuotationNumberTextBox.Text;
                quotation.CustomerId = (int)CustomerComboBox.SelectedValue;
                quotation.QuotationDate = QuotationDatePicker.SelectedDate ?? DateTime.Today;
                quotation.ValidUntilDate = ValidUntilDatePicker.SelectedDate ?? DateTime.Today.AddDays(30);
                quotation.Status = QuotationStatus.Draft;
                quotation.TotalAmount = _items.Sum(i => i.Subtotal);

                foreach (var item in _items)
                {
                    quotation.SalesQuotationItems.Add(new SalesQuotationItem
                    {
                        ProductId = item.ProductId,
                        Description = item.Description,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Discount = item.Discount
                    });
                }

                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}