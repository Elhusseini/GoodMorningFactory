// GoodMorningFactory/UI/ViewModels/AddEditPurchaseOrderViewModel.cs
// *** الكود الكامل والمعدل ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditPurchaseOrderViewModel : BaseViewModel
    {
        private readonly int? _poId;
        private readonly int? _sourceRequisitionId; // <-- إضافة حقل لتخزين معرف الطلب
        private readonly IPurchaseOrderService _poService;
        private PurchaseOrder _model;

        #region Properties
        private string _title;
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
        public ObservableCollection<PurchaseOrderItemViewModel> Items { get; set; }
        public System.Collections.Generic.List<Supplier> Suppliers { get; private set; }
        private Supplier _selectedSupplier;
        public Supplier SelectedSupplier { get => _selectedSupplier; set { _selectedSupplier = value; OnPropertyChanged(); SaveCommand.RaiseCanExecuteChanged(); } }
        public System.Collections.Generic.List<Product> Products { get; private set; }
        private DateTime _orderDate;
        public DateTime OrderDate { get => _orderDate; set { _orderDate = value; OnPropertyChanged(); } }
        private DateTime? _expectedDeliveryDate;
        public DateTime? ExpectedDeliveryDate { get => _expectedDeliveryDate; set { _expectedDeliveryDate = value; OnPropertyChanged(); } }
        private string _totalAmountText;
        public string TotalAmountText { get => _totalAmountText; set { _totalAmountText = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        public RelayCommand SaveCommand { get; }
        public RelayCommand AddItemCommand { get; }
        public RelayCommand RemoveItemCommand { get; }
        #endregion

        public AddEditPurchaseOrderViewModel(int? poId = null, int? sourceRequisitionId = null)
        {
            _poId = poId;
            _sourceRequisitionId = sourceRequisitionId; // <-- تخزين معرف الطلب
            _poService = new PurchaseOrderService();

            Items = new ObservableCollection<PurchaseOrderItemViewModel>();
            Items.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null) foreach (var item in e.NewItems.OfType<PurchaseOrderItemViewModel>()) item.PropertyChanged += Item_PropertyChanged;
                if (e.OldItems != null) foreach (var item in e.OldItems.OfType<PurchaseOrderItemViewModel>()) item.PropertyChanged -= Item_PropertyChanged;
                UpdateTotal();
            };

            SaveCommand = new RelayCommand(Save, CanSave);
            AddItemCommand = new RelayCommand(_ => AddNewItem());
            RemoveItemCommand = new RelayCommand(RemoveItem);

            LoadInitialData();
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) => UpdateTotal();

        private async void LoadInitialData()
        {
            try
            {
                PurchaseOrderDto dto;
                // ======================= بداية الإصلاح الرئيسي =======================
                // التحقق إذا كانت النافذة قد تم فتحها من طلب شراء
                if (_sourceRequisitionId.HasValue)
                {
                    Title = "إنشاء أمر شراء من طلب";
                    dto = await _poService.GetDataForPOFromRequisitionAsync(_sourceRequisitionId.Value);
                }
                else
                {
                    Title = _poId.HasValue ? "تعديل أمر شراء" : "إنشاء أمر شراء جديد";
                    dto = await _poService.GetDataForAddEditAsync(_poId);
                }
                // ======================== نهاية الإصلاح الرئيسي ========================

                _model = dto.Order;
                Suppliers = dto.AllSuppliers;
                Products = dto.AllProducts;
                OnPropertyChanged(nameof(Suppliers)); OnPropertyChanged(nameof(Products));

                SelectedSupplier = Suppliers.FirstOrDefault(s => s.Id == _model.SupplierId);
                OrderDate = _model.OrderDate;
                ExpectedDeliveryDate = _model.ExpectedDeliveryDate;

                Items.Clear();
                foreach (var item in _model.PurchaseOrderItems)
                {
                    Items.Add(new PurchaseOrderItemViewModel(item, Products));
                }
                if (!Items.Any())
                {
                    AddNewItem();
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ"); }
        }

        private void AddNewItem() => Items.Add(new PurchaseOrderItemViewModel(new PurchaseOrderItem { Quantity = 1 }, Products));
        private void RemoveItem(object parameter)
        {
            if (parameter is PurchaseOrderItemViewModel item) Items.Remove(item);
        }

        private void UpdateTotal()
        {
            decimal total = Items.Sum(i => i.Subtotal);
            TotalAmountText = $"{total:N2} {AppSettings.DefaultCurrencySymbol}";
            SaveCommand.RaiseCanExecuteChanged();
        }

        private bool CanSave(object obj) => SelectedSupplier != null && Items.Any(i => i.ProductId.HasValue && i.Quantity > 0);

        private async void Save(object parameter)
        {
            try
            {
                _model.SupplierId = SelectedSupplier.Id;
                _model.OrderDate = OrderDate;
                _model.ExpectedDeliveryDate = ExpectedDeliveryDate;
                _model.TotalAmount = Items.Sum(i => i.Subtotal);
                _model.Status = PurchaseOrderStatus.Sent;
                _model.PurchaseRequisitionId = _sourceRequisitionId; // <-- التأكد من حفظ الربط

                _model.PurchaseOrderItems.Clear();
                foreach (var vmItem in Items.Where(i => i.ProductId.HasValue))
                {
                    _model.PurchaseOrderItems.Add(vmItem.Model);
                }

                await _poService.SavePurchaseOrderAsync(_model);

                if (parameter is Window window) { window.DialogResult = true; window.Close(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل الحفظ: {ex.Message}", "خطأ");
            }
        }
    }
}