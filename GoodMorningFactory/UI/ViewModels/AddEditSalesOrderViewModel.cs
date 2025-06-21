using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditSalesOrderViewModel : BaseViewModel
    {
        private readonly ISalesOrderService _salesOrderService;
        private readonly IFilterService _filterService;
        private readonly ICustomerService _customerService;
        private readonly int? _orderId;

        #region Properties
        private string _windowTitle;
        public string WindowTitle { get => _windowTitle; set { _windowTitle = value; OnPropertyChanged(); } }

        private string _orderNumber;
        public string OrderNumber { get => _orderNumber; set { _orderNumber = value; OnPropertyChanged(); } }

        private DateTime _orderDate = DateTime.Today;
        public DateTime OrderDate { get => _orderDate; set { _orderDate = value; OnPropertyChanged(); } }

        private DateTime? _shipDate = DateTime.Today.AddDays(7);
        public DateTime? ShipDate { get => _shipDate; set { _shipDate = value; OnPropertyChanged(); } }

        private Customer _selectedCustomer;
        public Customer SelectedCustomer { get => _selectedCustomer; set { _selectedCustomer = value; OnPropertyChanged(); } }

        private PriceList _selectedPriceList;
        public PriceList SelectedPriceList { get => _selectedPriceList; set { _selectedPriceList = value; OnPropertyChanged(); } }

        public List<Customer> Customers { get; set; }
        public List<PriceList> PriceLists { get; set; }

        public ObservableCollection<SalesOrderItemViewModel> Items { get; set; }

        private string _productSearchText;
        public string ProductSearchText { get => _productSearchText; set { _productSearchText = value; OnPropertyChanged(); } }

        private string _totalAmountText;
        public string TotalAmountText { get => _totalAmountText; set { _totalAmountText = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand RemoveItemCommand { get; }
        #endregion

        public AddEditSalesOrderViewModel()
        {
            // مُنشئ فارغ لوقت التصميم
            WindowTitle = "إضافة / تعديل أمر بيع";
            Items = new ObservableCollection<SalesOrderItemViewModel>();
        }

        public AddEditSalesOrderViewModel(int? orderId = null, int? sourceQuotationId = null)
        {
            _salesOrderService = new SalesOrderService();
            _filterService = new FilterService();
            _customerService = new CustomerService();
            _orderId = orderId;

            Items = new ObservableCollection<SalesOrderItemViewModel>();

            Items.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (SalesOrderItemViewModel item in e.NewItems)
                        item.PropertyChanged += Item_PropertyChanged;
                if (e.OldItems != null)
                    foreach (SalesOrderItemViewModel item in e.OldItems)
                        item.PropertyChanged -= Item_PropertyChanged;
                UpdateTotal();
            };

            SaveCommand = new RelayCommand(SaveAsync);
            AddProductCommand = new RelayCommand(SearchAndAddProduct);
            RemoveItemCommand = new RelayCommand(RemoveItem);

            LoadInitialDataAsync(sourceQuotationId);
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SalesOrderItemViewModel.Subtotal))
            {
                UpdateTotal();
            }
        }

        private async void LoadInitialDataAsync(int? sourceQuotationId)
        {
            Customers = await _customerService.GetActiveCustomersAsync();
            PriceLists = await _filterService.GetPriceListsAsync();
            OnPropertyChanged(nameof(Customers));
            OnPropertyChanged(nameof(PriceLists));

            if (_orderId.HasValue)
            {
                WindowTitle = "تعديل أمر بيع";
                var order = await _salesOrderService.GetSalesOrderForEditAsync(_orderId.Value);
                if (order != null)
                {
                    OrderNumber = order.SalesOrderNumber;
                    SelectedCustomer = Customers.FirstOrDefault(c => c.Id == order.CustomerId);
                    OrderDate = order.OrderDate;
                    ShipDate = order.ExpectedShipDate;
                    foreach (var item in order.SalesOrderItems)
                    {
                        Items.Add(new SalesOrderItemViewModel
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
            else if (sourceQuotationId.HasValue)
            {
                WindowTitle = "إنشاء أمر بيع من عرض سعر";
                var quotation = await _salesOrderService.GetQuotationForConversionAsync(sourceQuotationId.Value);
                if (quotation != null)
                {
                    SelectedCustomer = Customers.FirstOrDefault(c => c.Id == quotation.CustomerId);
                    OrderNumber = await _salesOrderService.GetNextSalesOrderNumberAsync();
                    foreach (var item in quotation.SalesQuotationItems)
                    {
                        Items.Add(new SalesOrderItemViewModel
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
            else
            {
                WindowTitle = "إنشاء أمر بيع جديد";
                OrderNumber = await _salesOrderService.GetNextSalesOrderNumberAsync();
            }
        }

        private async void SearchAndAddProduct(object parameter)
        {
            if (string.IsNullOrWhiteSpace(ProductSearchText)) return;

            var product = await _salesOrderService.FindProductAsync(ProductSearchText);
            if (product != null)
            {
                var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);
                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    decimal price = await _salesOrderService.GetProductPriceAsync(product.Id, SelectedPriceList?.Id);
                    Items.Add(new SalesOrderItemViewModel
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Description = product.Description,
                        Quantity = 1,
                        UnitPrice = price
                    });
                }
                ProductSearchText = string.Empty;
            }
            else
            {
                MessageBox.Show("لم يتم العثور على المنتج.", "بحث");
            }
        }

        private void RemoveItem(object parameter)
        {
            if (parameter is SalesOrderItemViewModel item)
            {
                Items.Remove(item);
            }
        }

        private void UpdateTotal()
        {
            TotalAmountText = $"{Items.Sum(i => i.Subtotal):N2} {AppSettings.DefaultCurrencySymbol}";
        }

        private async void SaveAsync(object parameter)
        {
            if (SelectedCustomer == null)
            {
                MessageBox.Show("يرجى اختيار العميل.", "بيانات ناقصة");
                return;
            }

            decimal newOrderValue = Items.Sum(i => i.Subtotal);
            var (exceeded, message) = await _salesOrderService.CheckCreditLimitAsync(SelectedCustomer.Id, newOrderValue, _orderId);
            if (exceeded)
            {
                if (PermissionsService.CanAccess("Sales.OverrideCreditLimit"))
                {
                    var result = MessageBox.Show($"{message}\n\nهل تريد المتابعة على أي حال؟", "تحذير: تجاوز حد الائتمان", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.No) return;
                }
                else
                {
                    MessageBox.Show($"{message}\n\nليس لديك صلاحية لتجاوز حد الائتمان.", "عملية مرفوضة");
                    return;
                }
            }

            try
            {
                SalesOrder order;
                if (_orderId.HasValue)
                {
                    order = await _salesOrderService.GetSalesOrderForEditAsync(_orderId.Value);
                }
                else
                {
                    order = new SalesOrder();
                }

                order.SalesOrderNumber = OrderNumber;
                order.CustomerId = SelectedCustomer.Id;
                order.OrderDate = OrderDate;
                order.ExpectedShipDate = ShipDate;
                order.Status = OrderStatus.Confirmed;
                order.TotalAmount = newOrderValue;

                // Clear existing items only if it's an update
                if (order.Id > 0)
                {
                    order.SalesOrderItems.Clear();
                }

                foreach (var item in Items)
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

                await _salesOrderService.SaveSalesOrderAsync(order);

                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ أمر البيع: {ex.Message}", "خطأ");
            }
        }
    }
}
