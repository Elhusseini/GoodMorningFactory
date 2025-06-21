// UI/Views/LowStockNotificationsView.xaml.cs
// *** تحديث: تم إصلاح الاستعلام ليعتمد على الموقع الفرعي ***
using GoodMorningFactory.Data;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class LowStockNotificationsView : UserControl
    {
        public bool HasLowStockItems { get; private set; }

        public LowStockNotificationsView()
        {
            InitializeComponent();
            LoadLowStockItems();
        }

        private void LoadLowStockItems()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // *** بداية الإصلاح: تعديل الاستعلام ليشمل الموقع الفرعي والمخزن الرئيسي ***
                    var lowStockItems = db.Inventories
                        .Include(i => i.Product).ThenInclude(p => p.DefaultSupplier)
                        .Include(i => i.StorageLocation).ThenInclude(sl => sl.Warehouse) // الربط الجديد
                        .Where(i => i.Quantity <= i.ReorderLevel && i.ReorderLevel > 0)
                        .Select(i => new LowStockNotificationViewModel
                        {
                            ProductId = i.ProductId,
                            ProductCode = i.Product.ProductCode,
                            ProductName = i.Product.Name,
                            WarehouseName = i.StorageLocation.Warehouse.Name, // الوصول للمخزن عبر الموقع
                            QuantityOnHand = i.Quantity,
                            ReorderLevel = i.ReorderLevel,
                            DefaultSupplierName = i.Product.DefaultSupplier != null ? i.Product.DefaultSupplier.Name : "غير محدد"
                        })
                        .ToList();
                    // *** نهاية الإصلاح ***

                    LowStockDataGrid.ItemsSource = lowStockItems;
                    HasLowStockItems = lowStockItems.Any();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading low stock items: {ex.Message}");
                HasLowStockItems = false;
            }
        }
    }
}
