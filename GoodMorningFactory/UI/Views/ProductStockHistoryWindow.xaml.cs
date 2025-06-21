// UI/Views/ProductStockHistoryWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة سجل حركات المنتج ***
using GoodMorningFactory.Data;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class ProductStockHistoryWindow : Window
    {
        private readonly int _productId;

        public ProductStockHistoryWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;
            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var product = db.Products.Find(_productId);
                    if (product == null) { this.Close(); return; }

                    ProductNameTextBlock.Text = $"سجل حركات المنتج: {product.Name}";

                    // جلب جميع أنواع الحركات المتعلقة بالمنتج المحدد فقط
                    var goodsReceipts = db.GoodsReceiptNoteItems.Where(i => i.ProductId == _productId).Include(i => i.GoodsReceiptNote).Select(i => new StockMovementViewModel { Date = i.GoodsReceiptNote.ReceiptDate, TransactionType = "استلام بضاعة", ReferenceNumber = i.GoodsReceiptNote.GRNNumber, ProductName = product.Name, QuantityIn = i.QuantityReceived, QuantityOut = 0 });
                    var salesShipments = db.ShipmentItems.Where(i => i.ProductId == _productId).Include(i => i.Shipment).Select(i => new StockMovementViewModel { Date = i.Shipment.ShipmentDate, TransactionType = "مبيعات", ReferenceNumber = i.Shipment.ShipmentNumber, ProductName = product.Name, QuantityIn = 0, QuantityOut = i.Quantity });
                    var productionConsumption = db.WorkOrderMaterials.Where(m => m.RawMaterialId == _productId).Include(m => m.WorkOrder).Select(m => new StockMovementViewModel { Date = m.WorkOrder.ActualStartDate ?? m.WorkOrder.PlannedStartDate, TransactionType = "صرف إنتاج", ReferenceNumber = m.WorkOrder.WorkOrderNumber, ProductName = product.Name, QuantityIn = 0, QuantityOut = (int)m.QuantityConsumed });
                    var finishedGoodsProduction = db.WorkOrders.Where(wo => wo.FinishedGoodId == _productId && wo.QuantityProduced > 0 && wo.ActualEndDate.HasValue).Select(wo => new StockMovementViewModel { Date = wo.ActualEndDate.Value, TransactionType = "إنتاج مكتمل", ReferenceNumber = wo.WorkOrderNumber, ProductName = product.Name, QuantityIn = wo.QuantityProduced, QuantityOut = 0 });

                    var allMovements = goodsReceipts.Union(salesShipments).Union(productionConsumption).Union(finishedGoodsProduction);

                    HistoryDataGrid.ItemsSource = allMovements.OrderByDescending(m => m.Date).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل سجل الحركات: {ex.Message}", "خطأ");
            }
        }
    }
}