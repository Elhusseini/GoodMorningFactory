// UI/Views/AddEditSalesOrderWindow.xaml.cs
using GoodMorningFactory.Core.Services;
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
    public class SalesOrderItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }

        private decimal _unitPrice;
        public decimal UnitPrice { get => _unitPrice; set { if (_unitPrice != value) { _unitPrice = value; OnPropertyChanged(nameof(UnitPrice)); OnPropertyChanged(nameof(Subtotal)); OnPropertyChanged(nameof(SubtotalFormatted)); } } }

        private int _quantity;
        public int Quantity { get => _quantity; set { if (_quantity != value) { _quantity = value; OnPropertyChanged(nameof(Quantity)); OnPropertyChanged(nameof(Subtotal)); OnPropertyChanged(nameof(SubtotalFormatted)); } } }

        private decimal _discount;
        public decimal Discount { get => _discount; set { if (_discount != value) { _discount = value; OnPropertyChanged(nameof(Discount)); OnPropertyChanged(nameof(Subtotal)); OnPropertyChanged(nameof(SubtotalFormatted)); } } }

        public decimal Subtotal => (UnitPrice * Quantity) - Discount;
        public string SubtotalFormatted => $"{Subtotal:N2} {AppSettings.DefaultCurrencySymbol}";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    public partial class AddEditSalesOrderWindow : Window
    {
        private int? _orderId;
        private int? _sourceQuotationId;
        private ObservableCollection<SalesOrderItemViewModel> _items = new ObservableCollection<SalesOrderItemViewModel>();

        public AddEditSalesOrderWindow(int? orderId = null, int? sourceQuotationId = null)
        {
            InitializeComponent();
            _orderId = orderId;
            _sourceQuotationId = sourceQuotationId;
            OrderItemsDataGrid.ItemsSource = _items;
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

            if (_orderId.HasValue) // وضع التعديل
            {
                Title = "تعديل أمر بيع";
                using (var db = new DatabaseContext())
                {
                    var order = db.SalesOrders
                                  .Include(o => o.SalesOrderItems)
                                  .ThenInclude(i => i.Product)
                                  .FirstOrDefault(o => o.Id == _orderId.Value);
                    if (order != null)
                    {
                        OrderNumberTextBox.Text = order.SalesOrderNumber;
                        CustomerComboBox.SelectedValue = order.CustomerId;
                        OrderDatePicker.SelectedDate = order.OrderDate;
                        ShipDatePicker.SelectedDate = order.ExpectedShipDate;

                        foreach (var item in order.SalesOrderItems)
                        {
                            _items.Add(new SalesOrderItemViewModel
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
            else if (_sourceQuotationId.HasValue) // وضع التحويل من عرض سعر
            {
                Title = "إنشاء أمر بيع من عرض سعر";
                using (var db = new DatabaseContext())
                {
                    var quotation = db.SalesQuotations.Include(q => q.SalesQuotationItems).ThenInclude(i => i.Product)
                                       .FirstOrDefault(q => q.Id == _sourceQuotationId.Value);
                    if (quotation != null)
                    {
                        CustomerComboBox.SelectedValue = quotation.CustomerId;
                        CustomerComboBox.IsEnabled = false;
                        OrderDatePicker.SelectedDate = DateTime.Today;
                        ShipDatePicker.SelectedDate = DateTime.Today.AddDays(7);
                        OrderNumberTextBox.Text = $"SO-{DateTime.Now:yyyyMMddHHmmss}";

                        foreach (var item in quotation.SalesQuotationItems)
                        {
                            _items.Add(new SalesOrderItemViewModel
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
                Title = "إنشاء أمر بيع جديد";
                OrderDatePicker.SelectedDate = DateTime.Today;
                ShipDatePicker.SelectedDate = DateTime.Today.AddDays(7);
                OrderNumberTextBox.Text = $"SO-{DateTime.Now:yyyyMMddHHmmss}";
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
                    if (existingItem != null) { existingItem.Quantity++; }
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

                        _items.Add(new SalesOrderItemViewModel
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            Description = product.Description,
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
            string currencySymbol = Core.Services.AppSettings.DefaultCurrencySymbol;
            TotalAmountTextBlock.Text = $"{_items.Sum(i => i.Subtotal):N2} {currencySymbol}";
        }
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is SalesOrderItemViewModel item) _items.Remove(item);
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
                var customer = db.Customers.Find((int)CustomerComboBox.SelectedValue);
                if (customer != null && customer.CreditLimit > 0)
                {
                    decimal currentBalance = db.Sales
                        .Where(s => s.SalesOrder.CustomerId == customer.Id && s.Status != InvoiceStatus.Paid && s.Status != InvoiceStatus.Cancelled)
                        .Sum(s => s.TotalAmount - s.AmountPaid);

                    decimal newOrderValue = _items.Sum(i => i.Subtotal);

                    if ((currentBalance + newOrderValue) > customer.CreditLimit)
                    {
                        string message = $"قيمة هذا الأمر ستؤدي إلى تجاوز حد الائتمان للعميل '{customer.CustomerName}'.\n" +
                                         $"الرصيد الحالي: {currentBalance:C}\n" +
                                         $"قيمة الأمر الجديد: {newOrderValue:C}\n" +
                                         $"الإجمالي: {(currentBalance + newOrderValue):C}\n" +
                                         $"حد الائتمان: {customer.CreditLimit:C}";

                        if (PermissionsService.CanAccess("Sales.OverrideCreditLimit"))
                        {
                            var result = MessageBox.Show($"{message}\n\nهل تريد المتابعة على أي حال؟", "تحذير: تجاوز حد الائتمان", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (result == MessageBoxResult.No)
                            {
                                return;
                            }
                        }
                        else
                        {
                            MessageBox.Show($"{message}\n\nليس لديك صلاحية لتجاوز حد الائتمان. لا يمكن حفظ أمر البيع.", "عملية مرفوضة", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                SalesOrder order;
                if (_orderId.HasValue)
                {
                    order = db.SalesOrders.Include(o => o.SalesOrderItems).FirstOrDefault(o => o.Id == _orderId.Value);
                    if (order == null) return;
                    db.SalesOrderItems.RemoveRange(order.SalesOrderItems);
                }
                else
                {
                    order = new SalesOrder();
                    db.SalesOrders.Add(order);
                }

                order.SalesOrderNumber = OrderNumberTextBox.Text;
                order.CustomerId = (int)CustomerComboBox.SelectedValue;
                order.OrderDate = OrderDatePicker.SelectedDate ?? DateTime.Today;
                order.ExpectedShipDate = ShipDatePicker.SelectedDate;
                order.Status = OrderStatus.Confirmed;
                order.SalesQuotationId = _sourceQuotationId;
                order.TotalAmount = _items.Sum(i => i.Subtotal);

                foreach (var item in _items)
                {
                    order.SalesOrderItems.Add(new SalesOrderItem
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
