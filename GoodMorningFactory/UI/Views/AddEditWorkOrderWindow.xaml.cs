// UI/Views/AddEditWorkOrderWindow.xaml.cs
// *** الكود الكامل والنهائي: تم إصلاح منطق التحقق من توفر كميات المواد ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditWorkOrderWindow : Window
    {
        private int? _workOrderId;
        private int? _sourceSalesOrderItemId;
        private ObservableCollection<RequiredMaterialViewModel> _requiredMaterials = new ObservableCollection<RequiredMaterialViewModel>();
        private ObservableCollection<ConsumedMaterialViewModel> _consumedMaterials = new ObservableCollection<ConsumedMaterialViewModel>();

        public AddEditWorkOrderWindow(int? workOrderId = null, int? salesOrderItemId = null)
        {
            InitializeComponent();
            _workOrderId = workOrderId;
            _sourceSalesOrderItemId = salesOrderItemId;
            RequiredMaterialsDataGrid.ItemsSource = _requiredMaterials;
            ConsumedMaterialsDataGrid.ItemsSource = _consumedMaterials;
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            StatusComboBox.ItemsSource = Enum.GetValues(typeof(WorkOrderStatus));

            using (var db = new DatabaseContext())
            {
                FinishedGoodComboBox.ItemsSource = db.Products.Where(p => p.ProductType == ProductType.FinishedGood || p.ProductType == ProductType.WorkInProgress).ToList();
            }

            if (_workOrderId.HasValue)
            {
                Title = "عرض / تعديل أمر عمل";
                using (var db = new DatabaseContext())
                {
                    var wo = db.WorkOrders.Find(_workOrderId.Value);
                    if (wo != null)
                    {
                        WorkOrderNumberTextBox.Text = wo.WorkOrderNumber;
                        FinishedGoodComboBox.SelectedValue = wo.FinishedGoodId;
                        QuantityTextBox.Text = wo.QuantityToProduce.ToString();
                        StartDatePicker.SelectedDate = wo.PlannedStartDate;
                        EndDatePicker.SelectedDate = wo.PlannedEndDate;
                        StatusComboBox.SelectedItem = wo.Status;

                        var consumedItems = db.WorkOrderMaterials.Include(m => m.RawMaterial).Where(m => m.WorkOrderId == _workOrderId.Value).ToList();
                        foreach (var item in consumedItems)
                        {
                            _consumedMaterials.Add(new ConsumedMaterialViewModel { MaterialName = item.RawMaterial.Name, QuantityConsumed = item.QuantityConsumed });
                        }
                    }
                }
            }
            else if (_sourceSalesOrderItemId.HasValue)
            {
                Title = "إنشاء أمر عمل من أمر بيع";
                using (var db = new DatabaseContext())
                {
                    var salesItem = db.SalesOrderItems.Include(i => i.Product).FirstOrDefault(i => i.Id == _sourceSalesOrderItemId.Value);
                    if (salesItem != null)
                    {
                        FinishedGoodComboBox.SelectedValue = salesItem.ProductId;
                        QuantityTextBox.Text = salesItem.Quantity.ToString();
                        FinishedGoodComboBox.IsEnabled = false;
                        QuantityTextBox.IsEnabled = false;
                    }
                }
                WorkOrderNumberTextBox.Text = $"WO-SO-{DateTime.Now:yyyyMMddHHmmss}";
                StartDatePicker.SelectedDate = DateTime.Today;
                EndDatePicker.SelectedDate = DateTime.Today.AddDays(7);
                StatusComboBox.SelectedItem = WorkOrderStatus.Planned;
            }
            else
            {
                Title = "إنشاء أمر عمل جديد";
                WorkOrderNumberTextBox.Text = $"WO-{DateTime.Now:yyyyMMddHHmmss}";
                StartDatePicker.SelectedDate = DateTime.Today;
                EndDatePicker.SelectedDate = DateTime.Today.AddDays(7);
                StatusComboBox.SelectedItem = WorkOrderStatus.Planned;
            }
        }

        private void UpdateRequiredMaterials()
        {
            _requiredMaterials.Clear();
            if (FinishedGoodComboBox.SelectedItem == null || !int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0) return;
            int finishedGoodId = (int)FinishedGoodComboBox.SelectedValue;

            try
            {
                using (var db = new DatabaseContext())
                {
                    var bom = db.BillOfMaterials.Include(b => b.BillOfMaterialsItems).ThenInclude(i => i.RawMaterial).FirstOrDefault(b => b.FinishedGoodId == finishedGoodId);
                    if (bom == null) return;
                    foreach (var item in bom.BillOfMaterialsItems)
                    {
                        var totalAvailable = db.Inventories
                                               .Where(i => i.ProductId == item.RawMaterialId)
                                               .Sum(i => (int?)i.Quantity) ?? 0;

                        _requiredMaterials.Add(new RequiredMaterialViewModel
                        {
                            MaterialId = item.RawMaterialId,
                            MaterialName = item.RawMaterial.Name,
                            RequiredQuantity = item.Quantity * quantity,
                        });
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل في حساب المواد المطلوبة: {ex.Message}"); }
        }

        private void FinishedGoodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateRequiredMaterials();
        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateRequiredMaterials();

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (FinishedGoodComboBox.SelectedItem == null || !int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("يرجى اختيار المنتج وإدخال كمية صحيحة.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    if (!_workOrderId.HasValue)
                    {
                        var bom = await db.BillOfMaterials.Include(b => b.BillOfMaterialsItems)
                                        .FirstOrDefaultAsync(b => b.FinishedGoodId == (int)FinishedGoodComboBox.SelectedValue);

                        if (bom == null)
                        {
                            MessageBox.Show("لا توجد قائمة مواد (BOM) لهذا المنتج. لا يمكن إنشاء أمر العمل.", "خطأ");
                            return;
                        }

                        foreach (var item in bom.BillOfMaterialsItems)
                        {
                            var requiredQty = item.Quantity * quantity;
                            var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.RawMaterialId);
                            if (inventory == null || (inventory.Quantity - inventory.QuantityReserved) < requiredQty)
                            {
                                var material = await db.Products.FindAsync(item.RawMaterialId);
                                throw new Exception($"الكمية غير كافية للمادة: {material?.Name ?? "غير معروف"}. الرصيد المتاح: {inventory?.Quantity - inventory?.QuantityReserved ?? 0}.");
                            }
                        }

                        foreach (var item in bom.BillOfMaterialsItems)
                        {
                            var requiredQty = (int)Math.Ceiling(item.Quantity * quantity);
                            var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.RawMaterialId);
                            inventory.QuantityReserved += requiredQty;
                        }
                    }

                    WorkOrder wo;
                    if (_workOrderId.HasValue)
                    {
                        wo = await db.WorkOrders.FindAsync(_workOrderId.Value);
                    }
                    else
                    {
                        wo = new WorkOrder { WorkOrderNumber = WorkOrderNumberTextBox.Text };
                        db.WorkOrders.Add(wo);
                    }

                    wo.FinishedGoodId = (int)FinishedGoodComboBox.SelectedValue;
                    wo.QuantityToProduce = quantity;
                    wo.PlannedStartDate = StartDatePicker.SelectedDate ?? DateTime.Today;
                    wo.PlannedEndDate = EndDatePicker.SelectedDate ?? DateTime.Today.AddDays(7);

                    if (_sourceSalesOrderItemId.HasValue)
                    {
                        wo.SalesOrderItemId = _sourceSalesOrderItemId.Value;
                    }

                    if (wo.Id == 0)
                    {
                        wo.Status = WorkOrderStatus.Planned;
                    }

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    MessageBox.Show("تم حفظ أمر العمل بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    MessageBox.Show($"فشل حفظ أمر العمل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}