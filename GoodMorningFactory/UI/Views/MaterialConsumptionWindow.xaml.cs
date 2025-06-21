// UI/Views/MaterialConsumptionWindow.xaml.cs
// *** الكود الكامل للكود الخلفي لنافذة تسجيل استهلاك المواد مع الإصلاح ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class MaterialConsumptionWindow : Window
    {
        private readonly int _workOrderId;
        private ObservableCollection<RequiredMaterialViewModel> _itemsToConsume = new ObservableCollection<RequiredMaterialViewModel>();

        public MaterialConsumptionWindow(int workOrderId)
        {
            InitializeComponent();
            _workOrderId = workOrderId;
            MaterialsDataGrid.ItemsSource = _itemsToConsume;
            LoadWorkOrderData();
        }

        private void LoadWorkOrderData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var wo = db.WorkOrders.Find(_workOrderId);
                    if (wo == null) { this.Close(); return; }

                    WorkOrderNumberTextBlock.Text = $"أمر العمل رقم: {wo.WorkOrderNumber}";

                    var bom = db.BillOfMaterials.Include(b => b.BillOfMaterialsItems).ThenInclude(i => i.RawMaterial)
                                .FirstOrDefault(b => b.FinishedGoodId == wo.FinishedGoodId);

                    if (bom == null) { MessageBox.Show("لم يتم العثور على قائمة مكونات لهذا المنتج."); return; }

                    var previouslyConsumed = db.WorkOrderMaterials
                                               .Where(m => m.WorkOrderId == _workOrderId)
                                               .GroupBy(m => m.RawMaterialId)
                                               .ToDictionary(g => g.Key, g => g.Sum(i => i.QuantityConsumed));

                    foreach (var item in bom.BillOfMaterialsItems)
                    {
                        var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == item.RawMaterialId);
                        var requiredQty = item.Quantity * wo.QuantityToProduce;
                        decimal consumedQty = previouslyConsumed.ContainsKey(item.RawMaterialId) ? previouslyConsumed[item.RawMaterialId] : 0;

                        _itemsToConsume.Add(new RequiredMaterialViewModel
                        {
                            MaterialId = item.RawMaterialId,
                            MaterialName = item.RawMaterial.Name,
                            RequiredQuantity = requiredQty,
                            PreviouslyConsumedQuantity = consumedQty,
                            AvailableQuantity = inventory?.Quantity ?? 0,
                            ConsumedQuantity = 0
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}");
            }
        }

        private void ConfirmConsumptionButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    bool somethingToConsume = false;
                    foreach (var item in _itemsToConsume)
                    {
                        if (item.ConsumedQuantity > 0)
                        {
                            somethingToConsume = true;

                            if (item.ConsumedQuantity > item.RemainingToConsume)
                            {
                                throw new Exception($"لا يمكن صرف كمية أكبر من المتبقية للمادة: {item.MaterialName}");
                            }

                            var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == item.MaterialId);
                            if (inventory == null || inventory.Quantity < item.ConsumedQuantity)
                            {
                                throw new Exception($"الكمية غير كافية في المخزون للمادة: {item.MaterialName}");
                            }

                            inventory.Quantity -= (int)item.ConsumedQuantity;

                            db.WorkOrderMaterials.Add(new WorkOrderMaterial
                            {
                                WorkOrderId = _workOrderId,
                                RawMaterialId = item.MaterialId,
                                QuantityConsumed = item.ConsumedQuantity
                            });
                        }
                    }

                    if (!somethingToConsume)
                    {
                        MessageBox.Show("لم يتم إدخال أي كميات للاستهلاك.", "تنبيه");
                        return;
                    }

                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم تسجيل استهلاك المواد بنجاح.", "نجاح");
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ");
                }
            }
        }
    }
}