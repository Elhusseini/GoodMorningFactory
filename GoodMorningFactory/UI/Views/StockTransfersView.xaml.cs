// UI/Views/StockTransfersView.xaml.cs
// *** تحديث: تم إصلاح الاستعلام ليعتمد على الموقع الفرعي ***
using GoodMorningFactory.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class StockTransfersView : UserControl
    {
        public StockTransfersView()
        {
            InitializeComponent();
            LoadTransfers();
        }

        private void LoadTransfers()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // *** بداية الإصلاح: تعديل الاستعلام ليشمل الموقع الفرعي والمخزن الرئيسي ***
                    TransfersDataGrid.ItemsSource = db.StockTransfers
                        .Include(t => t.SourceStorageLocation.Warehouse) // الربط الجديد
                        .Include(t => t.DestinationStorageLocation.Warehouse) // الربط الجديد
                        .Include(t => t.User)
                        .OrderByDescending(t => t.TransferDate)
                        .Select(t => new
                        {
                            t.TransferNumber,
                            t.TransferDate,
                            SourceWarehouseName = t.SourceStorageLocation.Warehouse.Name,
                            DestinationWarehouseName = t.DestinationStorageLocation.Warehouse.Name,
                            UserName = t.User.Username
                        })
                        .ToList();
                    // *** نهاية الإصلاح ***
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل سجل التحويلات: {ex.Message}", "خطأ");
            }
        }

        private void NewTransferButton_Click(object sender, RoutedEventArgs e)
        {
            var transferWindow = new StockTransferWindow();
            if (transferWindow.ShowDialog() == true)
            {
                LoadTransfers();
            }
        }
    }
}
