// UI/Views/AddEditWorkOrderWindow.xaml.cs
// *** الكود الكامل لنافذة إنشاء وتعديل أمر عمل مع منطق عرض التفاصيل ***
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
        private ObservableCollection<RequiredMaterialViewModel> _requiredMaterials = new ObservableCollection<RequiredMaterialViewModel>();
        private ObservableCollection<ConsumedMaterialViewModel> _consumedMaterials = new ObservableCollection<ConsumedMaterialViewModel>();


        public AddEditWorkOrderWindow(int? workOrderId = null)
        {
            InitializeComponent();
            _workOrderId = workOrderId;
            RequiredMaterialsDataGrid.ItemsSource = _requiredMaterials;
            ConsumedMaterialsDataGrid.ItemsSource = _consumedMaterials; // <-- ربط الجدول الجديد
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            StatusComboBox.ItemsSource = Enum.GetValues(typeof(WorkOrderStatus));

            using (var db = new DatabaseContext())
            {
                FinishedGoodComboBox.ItemsSource = db.Products.Where(p => p.ProductType == ProductType.FinishedGood).ToList();
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

                        // --- بداية التحديث: تحميل المواد المصروفة ---
                        var consumedItems = db.WorkOrderMaterials
                                              .Include(m => m.RawMaterial)
                                              .Where(m => m.WorkOrderId == _workOrderId.Value)
                                              .ToList();

                        foreach (var item in consumedItems)
                        {
                            _consumedMaterials.Add(new ConsumedMaterialViewModel
                            {
                                MaterialName = item.RawMaterial.Name,
                                QuantityConsumed = item.QuantityConsumed
                            });
                        }
                        // --- نهاية التحديث ---
                    }
                }
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
            if (FinishedGoodComboBox.SelectedItem == null || !int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                return;
            }

            int finishedGoodId = (int)FinishedGoodComboBox.SelectedValue;

            try
            {
                using (var db = new DatabaseContext())
                {
                    var bom = db.BillOfMaterials.Include(b => b.BillOfMaterialsItems).ThenInclude(i => i.RawMaterial)
                                .FirstOrDefault(b => b.FinishedGoodId == finishedGoodId);

                    if (bom == null) return;

                    foreach (var item in bom.BillOfMaterialsItems)
                    {
                        var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == item.RawMaterialId);
                        _requiredMaterials.Add(new RequiredMaterialViewModel
                        {
                            MaterialName = item.RawMaterial.Name,
                            RequiredQuantity = item.Quantity * quantity,
                            AvailableQuantity = inventory?.Quantity ?? 0
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل في حساب المواد المطلوبة: {ex.Message}");
            }
        }

        private void FinishedGoodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRequiredMaterials();
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateRequiredMaterials();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (FinishedGoodComboBox.SelectedItem == null || !int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("يرجى اختيار المنتج وإدخال كمية صحيحة.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new DatabaseContext())
                {
                    WorkOrder wo;
                    if (_workOrderId.HasValue)
                    {
                        wo = db.WorkOrders.Find(_workOrderId.Value);
                        if (wo.Status != WorkOrderStatus.Planned)
                        {
                            MessageBox.Show("لا يمكن تعديل المنتج أو الكمية لأمر عمل قيد التنفيذ.", "عملية غير ممكنة", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
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
                    // wo.Status = (WorkOrderStatus)StatusComboBox.SelectedItem; // الحالة لا يتم تعديلها يدوياً

                    db.SaveChanges();
                    MessageBox.Show("تم حفظ أمر العمل بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ أمر العمل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}