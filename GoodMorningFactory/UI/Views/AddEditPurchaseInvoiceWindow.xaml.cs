// UI/Views/AddEditPurchaseInvoiceWindow.xaml.cs
// *** الكود الكامل للكود الخلفي لنافذة تسجيل فاتورة مورد مع جميع الإصلاحات ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public class PurchaseInvoiceItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        // (Implementation of OnPropertyChanged)
    }

    public partial class AddEditPurchaseInvoiceWindow : Window
    {
        private int? _purchaseOrderId;
        private int? _grnId;
        private ObservableCollection<PurchaseInvoiceItemViewModel> _items = new ObservableCollection<PurchaseInvoiceItemViewModel>();

        // --- بداية الإصلاح: تحديث المُنشئ ليقبل هوية مذكرة الاستلام ---
        public AddEditPurchaseInvoiceWindow(int? purchaseOrderId = null, int? grnId = null)
        {
            InitializeComponent();
            _purchaseOrderId = purchaseOrderId;
            _grnId = grnId;
            InvoiceDatePicker.SelectedDate = DateTime.Today;
            InvoiceItemsDataGrid.ItemsSource = _items;
            LoadInitialData();
        }
        // --- نهاية الإصلاح ---

        private void LoadInitialData()
        {
            using (var db = new DatabaseContext())
            {
                SupplierComboBox.ItemsSource = db.Suppliers.ToList();
                ProductColumn.ItemsSource = db.Products.ToList();

                if (_grnId.HasValue) // حالة إنشاء فاتورة من مذكرة استلام
                {
                    var grn = db.GoodsReceiptNotes
                               .Include(g => g.PurchaseOrder.Supplier)
                               .Include(g => g.GoodsReceiptNoteItems)
                               .ThenInclude(gi => gi.Product)
                               .FirstOrDefault(g => g.Id == _grnId.Value);
                    if (grn != null)
                    {
                        Title = $"إنشاء فاتورة لمذكرة الاستلام: {grn.GRNNumber}";
                        _purchaseOrderId = grn.PurchaseOrderId;
                        SupplierComboBox.SelectedValue = grn.PurchaseOrder.SupplierId;
                        SupplierComboBox.IsEnabled = false;

                        var poItems = db.PurchaseOrderItems.Where(i => i.PurchaseOrderId == grn.PurchaseOrderId).ToList();
                        foreach (var item in grn.GoodsReceiptNoteItems)
                        {
                            var poItem = poItems.FirstOrDefault(p => p.ProductId == item.ProductId);
                            _items.Add(new PurchaseInvoiceItemViewModel
                            {
                                ProductId = item.ProductId,
                                ProductName = item.Product.Name,
                                Quantity = item.QuantityReceived,
                                UnitPrice = poItem?.UnitPrice ?? 0
                            });
                        }
                        InvoiceItemsDataGrid.IsReadOnly = true;
                    }
                }
                // --- بداية التحديث: منطق ملء البيانات من أمر الشراء ---
                else if (_purchaseOrderId.HasValue)
                {
                    var po = db.PurchaseOrders
                               .Include(p => p.PurchaseOrderItems).ThenInclude(i => i.Product)
                               .FirstOrDefault(p => p.Id == _purchaseOrderId.Value);
                    if (po != null)
                    {
                        Title = $"إنشاء فاتورة لأمر الشراء: {po.PurchaseOrderNumber}";
                        SupplierComboBox.SelectedValue = po.SupplierId;
                        SupplierComboBox.IsEnabled = false; // لا يمكن تغيير المورد

                        // جلب بنود أمر الشراء وملء الجدول بها
                        foreach (var item in po.PurchaseOrderItems)
                        {
                            _items.Add(new PurchaseInvoiceItemViewModel
                            {
                                ProductId = item.ProductId,
                                ProductName = item.Product.Name,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice
                            });
                        }
                    }
                }
                // --- نهاية التحديث ---
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SupplierComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(InvoiceNumberTextBox.Text) || !_items.Any(i => i.ProductId > 0))
            {
                MessageBox.Show("يرجى اختيار مورد، إدخال رقم الفاتورة، وإضافة أصناف.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new DatabaseContext())
                {
                    var purchase = new Purchase
                    {
                        InvoiceNumber = InvoiceNumberTextBox.Text,
                        PurchaseDate = InvoiceDatePicker.SelectedDate ?? DateTime.Today,
                        DueDate = DueDatePicker.SelectedDate,
                        SupplierId = (int)SupplierComboBox.SelectedValue,
                        PurchaseOrderId = _purchaseOrderId,
                        GoodsReceiptNoteId = _grnId, // <-- ربط الفاتورة بمذكرة الاستلام
                        Status = PurchaseInvoiceStatus.ApprovedForPayment,
                        TotalAmount = _items.Sum(i => i.Quantity * i.UnitPrice),
                        AmountPaid = 0
                    };

                    foreach (var item in _items.Where(i => i.ProductId > 0 && i.Quantity > 0))
                    {
                        purchase.PurchaseItems.Add(new PurchaseItem { ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = item.UnitPrice });
                    }

                    if (_grnId.HasValue)
                    {
                        var grnToUpdate = db.GoodsReceiptNotes.Find(_grnId.Value);
                        if (grnToUpdate != null) { grnToUpdate.IsInvoiced = true; }
                    }
                    // --- بداية التحديث: تحديث حالة أمر الشراء إلى "مفوتر" ---
                 

                    db.Purchases.Add(purchase);
                    db.SaveChanges();
                    MessageBox.Show("تم تسجيل فاتورة المورد بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تسجيل الفاتورة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}