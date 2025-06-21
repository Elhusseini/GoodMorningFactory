// UI/Views/AddEditPurchaseOrderWindow.xaml.cs
// *** تحديث: تم إصلاح مشكلة تضارب المُنشئات ودمجها في مُنشئ واحد ***
// UI/Views/AddEditPurchaseOrderWindow.xaml.cs
// *** تحديث: تم إصلاح مشكلة تضارب المُنشئات ودمجها في مُنشئ واحد ***
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
    public class PurchaseOrderItemViewModel : INotifyPropertyChanged
    {
        public int? ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Description { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public partial class AddEditPurchaseOrderWindow : Window
    {
        private readonly int? _poId;
        private readonly int? _sourceRequisitionId;
        private readonly ObservableCollection<PurchaseOrderItemViewModel> _items = new ObservableCollection<PurchaseOrderItemViewModel>();

        // --- بداية الإصلاح: دمج المُنشئات في واحد باستخدام معاملات اختيارية ---
        public AddEditPurchaseOrderWindow(int? poId = null, int? sourceRequisitionId = null)
        {
            InitializeComponent();
            _poId = poId;
            _sourceRequisitionId = sourceRequisitionId;

            OrderDatePicker.SelectedDate = DateTime.Today;
            OrderItemsDataGrid.ItemsSource = _items;
            LoadInitialData();
        }
        // --- نهاية الإصلاح ---

        private void LoadInitialData()
        {
            using (var db = new DatabaseContext())
            {
                SupplierComboBox.ItemsSource = db.Suppliers.ToList();
                ProductColumn.ItemsSource = db.Products.ToList();
            }

            if (_poId.HasValue) // إذا كنا في وضع التعديل
            {
                Title = "تعديل أمر شراء";
                using (var db = new DatabaseContext())
                {
                    var po = db.PurchaseOrders.Include(p => p.PurchaseOrderItems)
                               .FirstOrDefault(p => p.Id == _poId.Value);
                    if (po != null)
                    {
                        SupplierComboBox.SelectedValue = po.SupplierId;
                        OrderDatePicker.SelectedDate = po.OrderDate;
                        foreach (var item in po.PurchaseOrderItems)
                        {
                            _items.Add(new PurchaseOrderItemViewModel { ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = item.UnitPrice });
                        }
                    }
                }
            }
            else if (_sourceRequisitionId.HasValue) // إذا كنا نقوم بالتحويل من طلب شراء
            {
                Title = "إنشاء أمر شراء من طلب";
                using (var db = new DatabaseContext())
                {
                    var req = db.PurchaseRequisitions.Include(r => r.PurchaseRequisitionItems).FirstOrDefault(r => r.Id == _sourceRequisitionId);
                    if (req != null)
                    {
                        RequisitionRefTextBlock.Text = req.RequisitionNumber;
                        RequisitionRefTextBlock.Visibility = Visibility.Visible;

                        foreach (var item in req.PurchaseRequisitionItems)
                        {
                            _items.Add(new PurchaseOrderItemViewModel { Description = item.Description, Quantity = (int)item.Quantity });
                        }
                    }
                }
            }
            else // إذا كنا في وضع الإضافة
            {
                Title = "إنشاء أمر شراء جديد";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SupplierComboBox.SelectedItem == null || !_items.Any(i => i.ProductId.HasValue && i.Quantity > 0 && i.UnitPrice > 0))
            {
                MessageBox.Show("يرجى اختيار مورد وإكمال بيانات الأصناف (المنتج، الكمية، السعر).", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    PurchaseOrder po;
                    if (_poId.HasValue)
                    {
                        po = db.PurchaseOrders.Include(p => p.PurchaseOrderItems).FirstOrDefault(p => p.Id == _poId.Value);
                        db.PurchaseOrderItems.RemoveRange(po.PurchaseOrderItems);
                    }
                    else
                    {
                        po = new PurchaseOrder { PurchaseOrderNumber = $"PO-{DateTime.Now:yyyyMMddHHmmss}" };
                        db.PurchaseOrders.Add(po);
                    }

                    po.OrderDate = OrderDatePicker.SelectedDate ?? DateTime.Today;
                    po.SupplierId = (int)SupplierComboBox.SelectedValue;
                    po.Status = PurchaseOrderStatus.Sent;
                    po.TotalAmount = _items.Sum(i => i.Quantity * i.UnitPrice);

                    foreach (var item in _items)
                    {
                        if (item.ProductId.HasValue)
                        {
                            po.PurchaseOrderItems.Add(new PurchaseOrderItem { ProductId = item.ProductId.Value, Quantity = item.Quantity, UnitPrice = item.UnitPrice });
                        }
                    }

                    if (_sourceRequisitionId.HasValue)
                    {
                        var req = db.PurchaseRequisitions.Find(_sourceRequisitionId.Value);
                        if (req != null) { req.Status = RequisitionStatus.PO_Created; }
                    }

                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم حفظ أمر الشراء بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشل إنشاء أمر الشراء: {ex.InnerException?.Message ?? ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}