// UI/Views/AddEditSalesQuotationWindow.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.Core.Services;
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
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (_unitPrice != value)
                {
                    _unitPrice = value;
                    OnPropertyChanged(nameof(UnitPrice));
                    OnPropertyChanged(nameof(Subtotal));
                    OnPropertyChanged(nameof(SubtotalFormatted));
                }
            }
        }

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
                    OnPropertyChanged(nameof(SubtotalFormatted));
                }
            }
        }

        private decimal _discount;
        public decimal Discount
        {
            get => _discount;
            set
            {
                if (_discount != value)
                {
                    _discount = value;
                    OnPropertyChanged(nameof(Discount));
                    OnPropertyChanged(nameof(Subtotal));
                    OnPropertyChanged(nameof(SubtotalFormatted));
                }
            }
        }

        public decimal Subtotal => (UnitPrice * Quantity) - Discount;
        public string SubtotalFormatted => $"{Subtotal:N2} {AppSettings.DefaultCurrencySymbol}";

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
                PriceListComboBox.ItemsSource = db.PriceLists.ToList();
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

        private void AddProduct()
        {
            if (string.IsNullOrWhiteSpace(SearchProductTextBox.Text)) return;

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
                        decimal price = product.SalePrice;
                        if (PriceListComboBox.SelectedValue is int priceListId)
                        {
                            var productPrice = db.ProductPrices.FirstOrDefault(pp => pp.PriceListId == priceListId && pp.ProductId == product.Id);
                            if (productPrice != null)
                            {
                                price = productPrice.Price;
                            }
                        }

                        _items.Add(new SalesQuotationItemViewModel
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            Description = product.Description ?? product.Name,
                            Quantity = 1,
                            UnitPrice = price,
                            Discount = 0
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

        private void SearchProductTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddProduct();
                e.Handled = true;
            }
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            AddProduct();
        }

        private void UpdateTotal()
        {
            string currencySymbol = AppSettings.DefaultCurrencySymbol;
            TotalAmountTextBlock.Text = $"{_items.Sum(i => i.Subtotal):N2} {currencySymbol}";
        }
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
                        Description = item.Description ?? item.ProductName,
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
