// UI/Views/ReportProductionWindow.xaml.cs
// *** تحديث: إضافة منطق تسجيل حركة "إدخال إنتاج تام" في السجل المركزي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class ReportProductionWindow : Window
    {
        private readonly int _workOrderId;
        private WorkOrder _workOrder;
        private int _maxProducibleQuantity;
        private int _remainingQuantityForOrder;

        public ReportProductionWindow(int workOrderId)
        {
            InitializeComponent();
            _workOrderId = workOrderId;
            LoadWorkOrderData();
        }

        private void LoadWorkOrderData()
        {
            using (var db = new DatabaseContext())
            {
                _workOrder = db.WorkOrders.Include(w => w.FinishedGood).FirstOrDefault(w => w.Id == _workOrderId);
                if (_workOrder != null)
                {
                    WorkOrderNumberTextBlock.Text = _workOrder.WorkOrderNumber;
                    ProductNameTextBlock.Text = _workOrder.FinishedGood.Name;
                    OrderedQuantityTextBlock.Text = _workOrder.QuantityToProduce.ToString();
                    PreviouslyProducedTextBlock.Text = _workOrder.QuantityProduced.ToString();

                    var bom = db.BillOfMaterials.Include(b => b.BillOfMaterialsItems).FirstOrDefault(b => b.FinishedGoodId == _workOrder.FinishedGoodId);
                    if (bom == null) { _maxProducibleQuantity = 0; return; }
                    var consumedMaterials = db.WorkOrderMaterials.Where(m => m.WorkOrderId == _workOrderId).GroupBy(m => m.RawMaterialId).ToDictionary(g => g.Key, g => g.Sum(i => i.QuantityConsumed));
                    int maxFromMaterials = int.MaxValue;
                    foreach (var bomItem in bom.BillOfMaterialsItems)
                    {
                        if (consumedMaterials.TryGetValue(bomItem.RawMaterialId, out decimal totalConsumed))
                        {
                            if (bomItem.Quantity == 0) continue;
                            int producible = (int)(totalConsumed / bomItem.Quantity);
                            if (producible < maxFromMaterials) { maxFromMaterials = producible; }
                        }
                        else { maxFromMaterials = 0; break; }
                    }
                    _maxProducibleQuantity = maxFromMaterials - _workOrder.QuantityProduced;
                    _remainingQuantityForOrder = _workOrder.QuantityToProduce - _workOrder.QuantityProduced;
                    RemainingQuantityTextBlock.Text = _remainingQuantityForOrder.ToString();
                }
            }
        }

        private void ConfirmProductionButton_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(ProducedQuantityTextBox.Text, out int producedQuantity);
            int.TryParse(ScrappedQuantityTextBox.Text, out int scrappedQuantity);
            string scrapReason = ScrapReasonTextBox.Text;

            if (producedQuantity < 0 || scrappedQuantity < 0)
            {
                MessageBox.Show("لا يمكن إدخال كميات سالبة.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int totalReported = producedQuantity + scrappedQuantity;
            if (totalReported > _remainingQuantityForOrder)
            {
                MessageBox.Show($"إجمالي الكمية المسجلة ({totalReported}) أكبر من الكمية المتبقية المطلوبة في أمر العمل ({_remainingQuantityForOrder}).", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (totalReported > _maxProducibleQuantity)
            {
                MessageBox.Show($"إجمالي الكمية المسجلة ({totalReported}) أكبر من الكمية الممكن إنتاجها بالمواد المصروفة ({_maxProducibleQuantity}).", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // نفترض وجود موقع تخزين افتراضي للمنتج النهائي
                    var defaultLocation = db.StorageLocations.FirstOrDefault();
                    if (defaultLocation == null)
                    {
                        throw new Exception("لا يوجد موقع تخزين افتراضي معرف في النظام لاستلام المنتجات النهائية.");
                    }

                    if (producedQuantity > 0)
                    {
                        var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == _workOrder.FinishedGoodId && i.StorageLocationId == defaultLocation.Id);
                        if (inventory != null)
                        {
                            inventory.Quantity += producedQuantity;
                        }
                        else
                        {
                            db.Inventories.Add(new Inventory { ProductId = _workOrder.FinishedGoodId, StorageLocationId = defaultLocation.Id, Quantity = producedQuantity });
                        }

                        // --- بداية الإضافة: تسجيل حركة إدخال الإنتاج ---
                        var product = db.Products.Find(_workOrder.FinishedGoodId);
                        db.StockMovements.Add(new StockMovement
                        {
                            ProductId = _workOrder.FinishedGoodId,
                            StorageLocationId = defaultLocation.Id,
                            MovementDate = DateTime.Now,
                            MovementType = StockMovementType.FinishedGoodsProduction,
                            Quantity = producedQuantity,
                            UnitCost = product.AverageCost, // التكلفة ستكون قد تم تحديثها من عمليات سابقة
                            ReferenceDocument = _workOrder.WorkOrderNumber,
                            UserId = CurrentUserService.LoggedInUser?.Id
                        });
                        // --- نهاية الإضافة ---
                    }

                    if (scrappedQuantity > 0)
                    {
                        db.WorkOrderScraps.Add(new WorkOrderScrap
                        {
                            WorkOrderId = _workOrderId,
                            ProductId = _workOrder.FinishedGoodId,
                            Quantity = scrappedQuantity,
                            Reason = scrapReason
                        });
                    }

                    var orderToUpdate = db.WorkOrders.Find(_workOrderId);
                    if (orderToUpdate != null)
                    {
                        orderToUpdate.QuantityProduced += producedQuantity;
                        orderToUpdate.QuantityScrapped += scrappedQuantity;

                        if ((orderToUpdate.QuantityProduced + orderToUpdate.QuantityScrapped) >= orderToUpdate.QuantityToProduce)
                        {
                            orderToUpdate.Status = WorkOrderStatus.Completed;
                            orderToUpdate.ActualEndDate = DateTime.Now;
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم تسجيل الإنتاج بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    string errorMessage = $"فشلت عملية تسجيل الإنتاج: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\nالتفاصيل: {ex.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
