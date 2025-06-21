// UI/Views/ProductStockHistoryWindow.xaml.cs
// *** تحديث: تم إصلاح الكود ليعتمد على الـ ViewModel المركزي والجدول الجديد ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
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

                    // --- بداية الإصلاح: الاستعلام من جدول StockMovements المركزي ---
                    var movements = db.StockMovements
                        .Where(m => m.ProductId == _productId)
                        .Include(m => m.StorageLocation.Warehouse)
                        .Include(m => m.User)
                        .OrderByDescending(m => m.MovementDate)
                        .ToList();

                    var viewModels = movements.Select(m => new StockMovementViewModel
                    {
                        Date = m.MovementDate,
                        MovementType = m.MovementType,
                        ReferenceNumber = m.ReferenceDocument,
                        ProductName = product.Name,
                        WarehouseName = m.StorageLocation.Warehouse.Name,
                        StorageLocationName = m.StorageLocation.Name,
                        QuantityIn = (m.MovementType == StockMovementType.PurchaseReceipt || m.MovementType == StockMovementType.FinishedGoodsProduction || m.MovementType == StockMovementType.AdjustmentIncrease || m.MovementType == StockMovementType.TransferIn || m.MovementType == StockMovementType.SalesReturn) ? m.Quantity : 0,
                        QuantityOut = (m.MovementType == StockMovementType.SalesShipment || m.MovementType == StockMovementType.ProductionConsumption || m.MovementType == StockMovementType.AdjustmentDecrease || m.MovementType == StockMovementType.TransferOut || m.MovementType == StockMovementType.PurchaseReturn) ? m.Quantity : 0,
                        UserName = m.User?.Username ?? "System"
                    }).ToList();

                    HistoryDataGrid.ItemsSource = viewModels;
                    // --- نهاية الإصلاح ---
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل سجل الحركات: {ex.Message}", "خطأ");
            }
        }
    }
}
