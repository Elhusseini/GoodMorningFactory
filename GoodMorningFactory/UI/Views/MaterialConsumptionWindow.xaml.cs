// UI/Views/MaterialConsumptionWindow.xaml.cs
// *** تحديث: تم إصلاح خطأ في تعيين الموقع المصدر ***
using GoodMorningFactory.Core.Services;
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

                    var bom = db.BillOfMaterials.Include(b => b.BillOfMaterialsItems).ThenInclude(i => i.RawMaterial).FirstOrDefault(b => b.FinishedGoodId == wo.FinishedGoodId);
                    if (bom == null) { MessageBox.Show("لم يتم العثور على قائمة مكونات لهذا المنتج."); return; }

                    var previouslyConsumed = db.WorkOrderMaterials
                                               .Where(m => m.WorkOrderId == _workOrderId)
                                               .GroupBy(m => m.RawMaterialId)
                                               .ToDictionary(g => g.Key, g => g.Sum(i => i.QuantityConsumed));

                    foreach (var item in bom.BillOfMaterialsItems)
                    {
                        var requiredQty = item.Quantity * wo.QuantityToProduce;
                        decimal consumedQty = previouslyConsumed.ContainsKey(item.RawMaterialId) ? previouslyConsumed[item.RawMaterialId] : 0;

                        var availableStock = db.Inventories
                            .Include(i => i.StorageLocation)
                            .Where(i => i.ProductId == item.RawMaterialId && i.Quantity > 0)
                            .Select(i => new AvailableStockLocation
                            {
                                StorageLocationId = i.StorageLocationId,
                                LocationName = i.StorageLocation.Name,
                                QuantityOnHand = i.Quantity
                            }).ToList();

                        _itemsToConsume.Add(new RequiredMaterialViewModel
                        {
                            MaterialId = item.RawMaterialId,
                            MaterialName = item.RawMaterial.Name,
                            RequiredQuantity = requiredQty,
                            PreviouslyConsumedQuantity = consumedQty,
                            AvailableLocations = availableStock,
                            ConsumedQuantity = 0,
                            // --- بداية الإصلاح: استخدام الخاصية الصحيحة StorageLocationId ---
                            SourceLocationId = availableStock.FirstOrDefault()?.StorageLocationId,
                            // --- نهاية الإصلاح ---
                            IsTracked = item.RawMaterial.TrackingMethod != ProductTrackingMethod.None,
                            TrackingMethod = item.RawMaterial.TrackingMethod
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}");
            }
        }

        private void SelectTrackingDataButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is RequiredMaterialViewModel item)
            {
                if (item.ConsumedQuantity <= 0)
                {
                    MessageBox.Show("يرجى تحديد الكمية المراد صرفها أولاً.", "تنبيه");
                    return;
                }
                if (!item.SourceLocationId.HasValue)
                {
                    MessageBox.Show("يرجى تحديد الموقع المصدر أولاً.", "تنبيه");
                    return;
                }

                var selectionWindow = new SelectTrackingDataWindow(item.MaterialId, item.SourceLocationId.Value, (int)item.ConsumedQuantity, item.TrackingMethod);
                if (selectionWindow.ShowDialog() == true)
                {
                    item.SelectedSerialIds = selectionWindow.SelectedIds;
                    MessageBox.Show($"تم اختيار {item.SelectedSerialIds.Count} رقم بنجاح.", "نجاح");
                }
            }
        }

        private async void ConfirmConsumptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_itemsToConsume.Any(item => item.ConsumedQuantity > 0 && item.SourceLocationId == null))
            {
                MessageBox.Show("يرجى تحديد الموقع المصدر لكل مادة سيتم صرفها.", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var workOrder = await db.WorkOrders.FindAsync(_workOrderId);
                    if (workOrder == null) throw new Exception("لم يتم العثور على أمر العمل.");

                    bool somethingToConsume = false;
                    foreach (var item in _itemsToConsume)
                    {
                        if (item.ConsumedQuantity > 0)
                        {
                            somethingToConsume = true;

                            var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.MaterialId && i.StorageLocationId == item.SourceLocationId.Value);
                            if (inventory == null || inventory.Quantity < item.ConsumedQuantity)
                            {
                                throw new Exception($"الكمية غير كافية في الموقع المحدد للمادة: {item.MaterialName}");
                            }

                            if (item.IsTracked)
                            {
                                if (item.TrackingMethod == ProductTrackingMethod.BySerialNumber && item.SelectedSerialIds.Count != (int)item.ConsumedQuantity)
                                {
                                    throw new Exception($"لم يتم اختيار الأرقام التسلسلية بشكل صحيح للمادة: {item.MaterialName}");
                                }

                                var serialsToUpdate = await db.SerialNumbers.Where(sn => item.SelectedSerialIds.Contains(sn.Id)).ToListAsync();
                                foreach (var serial in serialsToUpdate)
                                {
                                    serial.Status = SerialNumberStatus.Consumed;
                                }
                            }

                            inventory.Quantity -= (int)item.ConsumedQuantity;

                            db.WorkOrderMaterials.Add(new WorkOrderMaterial
                            {
                                WorkOrderId = _workOrderId,
                                RawMaterialId = item.MaterialId,
                                QuantityConsumed = item.ConsumedQuantity
                            });

                            var product = await db.Products.FindAsync(item.MaterialId);
                            db.StockMovements.Add(new StockMovement
                            {
                                ProductId = item.MaterialId,
                                StorageLocationId = item.SourceLocationId.Value,
                                MovementDate = DateTime.Now,
                                MovementType = StockMovementType.ProductionConsumption,
                                Quantity = (int)item.ConsumedQuantity,
                                UnitCost = product.AverageCost,
                                ReferenceDocument = workOrder.WorkOrderNumber,
                                UserId = CurrentUserService.LoggedInUser?.Id
                            });
                        }
                    }

                    if (!somethingToConsume)
                    {
                        MessageBox.Show("لم يتم إدخال أي كميات للاستهلاك.", "تنبيه");
                        return;
                    }

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    MessageBox.Show("تم تسجيل استهلاك المواد بنجاح.", "نجاح");
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ");
                }
            }
        }
    }
}
