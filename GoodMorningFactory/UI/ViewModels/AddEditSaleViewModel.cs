// GoodMorningFactory/UI/ViewModels/AddEditSaleViewModel.cs
// *** الكود الكامل والنهائي للنافذة الموحدة ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditSaleViewModel : BaseViewModel
    {
        private readonly ISalesService _salesService;
        private readonly int? _saleId;
        private Sale _saleModel;

        #region Properties
        public string WindowTitle { get; private set; }
        public ObservableCollection<EditSaleItemViewModel> Items { get; } = new ObservableCollection<EditSaleItemViewModel>();
        public ObservableCollection<Customer> Customers { get; } = new ObservableCollection<Customer>();
        public ObservableCollection<Product> AllProducts { get; } = new ObservableCollection<Product>();

        private Customer _selectedCustomer;
        public Customer SelectedCustomer { get => _selectedCustomer; set { _selectedCustomer = value; OnPropertyChanged(); (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }

        private DateTime _saleDate;
        public DateTime SaleDate { get => _saleDate; set { _saleDate = value; OnPropertyChanged(); } }

        private decimal _amountPaid;
        public decimal AmountPaid { get => _amountPaid; set { _amountPaid = value; OnPropertyChanged(); } }

        private string _totalAmountText;
        public string TotalAmountText { get => _totalAmountText; set { _totalAmountText = value; OnPropertyChanged(); } }

        private string _searchProductText;
        public string SearchProductText { get => _searchProductText; set { _searchProductText = value; OnPropertyChanged(); } }
        #endregion

        public ICommand SaveCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }

        public AddEditSaleViewModel(int? saleId)
        {
            _saleId = saleId;
            _salesService = new SalesService();
            Items.CollectionChanged += (s, e) => { UpdateTotal(); (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged(); };
            SaveCommand = new RelayCommand(async (p) => await SaveAsync(p as Window), (p) => Items.Any() && SelectedCustomer != null);
            AddItemCommand = new RelayCommand(AddItem);
            RemoveItemCommand = new RelayCommand(RemoveItem);
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var customers = await db.Customers.ToListAsync();
                    var products = await db.Products.ToListAsync();
                    Customers.Clear(); customers.ForEach(c => Customers.Add(c));
                    AllProducts.Clear(); products.ForEach(p => AllProducts.Add(p));
                }

                if (_saleId.HasValue)
                {
                    WindowTitle = "تعديل فاتورة بيع";
                    _saleModel = await _salesService.GetSaleForEditAsync(_saleId.Value);
                    if (_saleModel == null) { MessageBox.Show("لم يتم العثور على الفاتورة."); return; }
                    SelectedCustomer = Customers.FirstOrDefault(c => c.Id == _saleModel.CustomerId);
                    SaleDate = _saleModel.SaleDate;
                    AmountPaid = _saleModel.AmountPaid;
                    Items.Clear();
                    foreach (var item in _saleModel.SaleItems)
                    {
                        Items.Add(new EditSaleItemViewModel { ProductId = item.ProductId, ProductName = item.Product.Name, Quantity = item.Quantity, UnitPrice = item.UnitPrice });
                    }
                }
                else
                {
                    WindowTitle = "فاتورة بيع جديدة";
                    _saleModel = new Sale();
                    SaleDate = DateTime.Today;
                }
                UpdateTotal();
                OnPropertyChanged(nameof(WindowTitle));
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ"); }
        }

        private void AddItem(object parameter)
        {
            if (string.IsNullOrWhiteSpace(SearchProductText)) return;
            var product = AllProducts.FirstOrDefault(p => p.ProductCode.ToLower() == SearchProductText.ToLower() || p.Name.ToLower().Contains(SearchProductText.ToLower()));
            if (product != null)
            {
                var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);
                if (existingItem != null) { existingItem.Quantity++; }
                else { Items.Add(new EditSaleItemViewModel { ProductId = product.Id, ProductName = product.Name, Quantity = 1, UnitPrice = product.SalePrice }); }
                SearchProductText = string.Empty;
            }
        }

        private void RemoveItem(object parameter)
        {
            if (parameter is EditSaleItemViewModel item) Items.Remove(item);
        }

        private void UpdateTotal()
        {
            TotalAmountText = $"{Items.Sum(i => i.Subtotal):N2} {AppSettings.DefaultCurrencySymbol}";
        }

        private async Task SaveAsync(Window window)
        {
            try
            {
                _saleModel.SaleDate = SaleDate;
                _saleModel.CustomerId = SelectedCustomer.Id;
                var newItems = Items.Select(vm => new SaleItem { ProductId = vm.ProductId, Quantity = vm.Quantity, UnitPrice = vm.UnitPrice }).ToList();

                if (_saleId.HasValue) { await _salesService.UpdateSaleAsync(_saleModel, newItems, AmountPaid); }
                else { await _salesService.AddSaleAsync(_saleModel, newItems, AmountPaid); }

                MessageBox.Show("تم حفظ الفاتورة بنجاح.");
                window.DialogResult = true;
            }
            catch (Exception ex) { MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ"); }
        }
    }
}