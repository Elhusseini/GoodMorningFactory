// GoodMorningFactory/UI/ViewModels/AddEditPurchaseInvoiceViewModel.cs
// *** الكود الكامل والشامل والنهائي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditPurchaseInvoiceViewModel : BaseViewModel
    {
        private readonly IPurchaseService _purchaseService;
        private readonly int? _purchaseId;
        private readonly int? _sourcePurchaseOrderId;
        private readonly int? _sourceGrnId;
        private Purchase _model;
        private readonly List<int> _grnIdsToInvoice = new List<int>();

        #region Properties
        public string Title { get; private set; }
        public List<Supplier> AllSuppliers { get; private set; }
        public List<Product> AllProducts { get; private set; }
        private Supplier _selectedSupplier;
        public Supplier SelectedSupplier { get => _selectedSupplier; set { _selectedSupplier = value; OnPropertyChanged(); SaveCommand.RaiseCanExecuteChanged(); } }
        private string _invoiceNumber;
        public string InvoiceNumber { get => _invoiceNumber; set { _invoiceNumber = value; OnPropertyChanged(); SaveCommand.RaiseCanExecuteChanged(); } }
        private DateTime _invoiceDate;
        public DateTime InvoiceDate { get => _invoiceDate; set { _invoiceDate = value; OnPropertyChanged(); } }
        private DateTime? _dueDate;
        public DateTime? DueDate { get => _dueDate; set { _dueDate = value; OnPropertyChanged(); } }
        public ObservableCollection<PurchaseInvoiceItemViewModel> Items { get; set; }
        private string _totalAmountText;
        public string TotalAmountText { get => _totalAmountText; set { _totalAmountText = value; OnPropertyChanged(); } }
        private string _productSearchText;
        public string ProductSearchText { get => _productSearchText; set { _productSearchText = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        public RelayCommand SaveCommand { get; }
        public RelayCommand AddItemCommand { get; }
        public RelayCommand RemoveItemCommand { get; }
        public RelayCommand SearchAndAddItemCommand { get; }
        #endregion

        public AddEditPurchaseInvoiceViewModel(int? purchaseId = null, int? purchaseOrderId = null, int? grnId = null)
        {
            _purchaseService = new PurchaseService();
            _purchaseId = purchaseId;
            _sourcePurchaseOrderId = purchaseOrderId;
            _sourceGrnId = grnId;

            Items = new ObservableCollection<PurchaseInvoiceItemViewModel>();
            Items.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null) foreach (var item in e.NewItems.OfType<PurchaseInvoiceItemViewModel>()) item.PropertyChanged += (s, a) => UpdateTotal();
                if (e.OldItems != null) foreach (var item in e.OldItems.OfType<PurchaseInvoiceItemViewModel>()) item.PropertyChanged -= (s, a) => UpdateTotal();
                UpdateTotal();
            };

            SaveCommand = new RelayCommand(Save, CanSave);
            AddItemCommand = new RelayCommand(_ => AddNewItem());
            RemoveItemCommand = new RelayCommand(RemoveItem);
            SearchAndAddItemCommand = new RelayCommand(SearchAndAddItem);

            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                var dto = await _purchaseService.GetDataForAddEditAsync(_purchaseId, _sourcePurchaseOrderId, _sourceGrnId);
                _model = dto.Purchase;
                AllSuppliers = dto.AllSuppliers;
                AllProducts = dto.AllProducts;

                _grnIdsToInvoice.Clear();
                if (dto.SourceGrnIds != null)
                {
                    _grnIdsToInvoice.AddRange(dto.SourceGrnIds);
                }

                OnPropertyChanged(nameof(AllSuppliers));
                OnPropertyChanged(nameof(AllProducts));

                if (_purchaseId.HasValue && _model != null)
                {
                    Title = "تعديل فاتورة شراء";
                    SelectedSupplier = AllSuppliers.FirstOrDefault(s => s.Id == _model.SupplierId);
                    InvoiceNumber = _model.InvoiceNumber;
                    InvoiceDate = _model.PurchaseDate;
                    DueDate = _model.DueDate;
                    foreach (var item in _model.PurchaseItems)
                    {
                        Items.Add(new PurchaseInvoiceItemViewModel { ProductId = item.ProductId, ProductName = AllProducts.FirstOrDefault(p => p.Id == item.ProductId)?.Name, Quantity = item.Quantity, UnitPrice = item.UnitPrice });
                    }
                }
                else
                {
                    Title = "إنشاء فاتورة شراء جديدة";
                    if (_model == null) _model = new Purchase();

                    SelectedSupplier = AllSuppliers.FirstOrDefault(s => s.Id == _model.SupplierId);
                    InvoiceNumber = $"PINV-{DateTime.Now:yyyyMMddHHmmss}";
                    InvoiceDate = DateTime.Today;
                    foreach (var item in _model.PurchaseItems)
                    {
                        Items.Add(new PurchaseInvoiceItemViewModel { ProductId = item.ProductId, ProductName = AllProducts.FirstOrDefault(p => p.Id == item.ProductId)?.Name, Quantity = item.Quantity, UnitPrice = item.UnitPrice });
                    }
                    if (!Items.Any()) AddNewItem();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        private void SearchAndAddItem(object parameter)
        {
            if (string.IsNullOrWhiteSpace(ProductSearchText)) return;
            var product = AllProducts.FirstOrDefault(p => p.ProductCode.Equals(ProductSearchText, StringComparison.OrdinalIgnoreCase) || p.Name.Contains(ProductSearchText));
            if (product != null)
            {
                var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);
                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    var lastEmptyItem = Items.FirstOrDefault(i => i.ProductId == 0);
                    if (lastEmptyItem != null)
                    {
                        lastEmptyItem.ProductId = product.Id;
                        lastEmptyItem.ProductName = product.Name;
                        lastEmptyItem.Quantity = 1;
                        lastEmptyItem.UnitPrice = product.PurchasePrice;
                    }
                    else
                    {
                        Items.Add(new PurchaseInvoiceItemViewModel { ProductId = product.Id, ProductName = product.Name, Quantity = 1, UnitPrice = product.PurchasePrice });
                    }
                }
                ProductSearchText = string.Empty;
            }
            else
            {
                MessageBox.Show("لم يتم العثور على المنتج.");
            }
        }

        private void AddNewItem() => Items.Add(new PurchaseInvoiceItemViewModel { Quantity = 1 });
        private void RemoveItem(object parameter)
        {
            if (parameter is PurchaseInvoiceItemViewModel item) Items.Remove(item);
        }

        private void UpdateTotal()
        {
            decimal total = Items.Sum(i => i.Subtotal);
            TotalAmountText = $"{total:N2} {AppSettings.DefaultCurrencySymbol}";
            SaveCommand.RaiseCanExecuteChanged();
        }

        private bool CanSave(object obj) => SelectedSupplier != null && !string.IsNullOrWhiteSpace(InvoiceNumber) && Items.Any(i => i.ProductId > 0 && i.Quantity > 0);

        private async void Save(object parameter)
        {
            _model.SupplierId = SelectedSupplier.Id;
            _model.InvoiceNumber = InvoiceNumber;
            _model.PurchaseDate = InvoiceDate;
            _model.DueDate = DueDate;
            _model.Status = PurchaseInvoiceStatus.ApprovedForPayment;
            _model.PurchaseOrderId = _model.PurchaseOrderId ?? _sourcePurchaseOrderId;

            try
            {
                await _purchaseService.SavePurchaseAsync(_model, Items.Where(i => i.ProductId > 0).ToList(), _grnIdsToInvoice);
                MessageBox.Show("تم حفظ الفاتورة بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                if (parameter is Window window) { window.DialogResult = true; window.Close(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل الحفظ: {ex.Message}", "خطأ");
            }
        }
    }
}