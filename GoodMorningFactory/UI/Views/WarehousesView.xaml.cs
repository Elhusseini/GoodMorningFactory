// UI/Views/WarehousesView.xaml.cs
// *** تحديث شامل: تم تعديل الواجهة لعرض وإدارة المخازن والمواقع الفرعية ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class WarehousesView : UserControl
    {
        public WarehousesView()
        {
            InitializeComponent();
            LoadWarehouses();
        }

        private void LoadWarehouses()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    WarehousesDataGrid.ItemsSource = db.Warehouses.OrderBy(w => w.Name).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المخازن: {ex.Message}");
            }
        }

        private void WarehousesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WarehousesDataGrid.SelectedItem is Warehouse selectedWarehouse)
            {
                LocationsGroupBox.IsEnabled = true;
                LocationsGroupBox.Header = $"المواقع الفرعية في: {selectedWarehouse.Name}";
                LoadLocationsForWarehouse(selectedWarehouse.Id);
            }
            else
            {
                LocationsGroupBox.IsEnabled = false;
                LocationsGroupBox.Header = "المواقع الفرعية";
                LocationsDataGrid.ItemsSource = null;
            }
        }

        private void LoadLocationsForWarehouse(int warehouseId)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    LocationsDataGrid.ItemsSource = db.StorageLocations
                        .Where(l => l.WarehouseId == warehouseId)
                        .OrderBy(l => l.Name)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل المواقع الفرعية: {ex.Message}", "خطأ");
            }
        }

        // --- دوال المخازن الرئيسية ---
        private void AddWarehouseButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditWarehouseWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadWarehouses();
            }
        }

        private void EditWarehouseButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Warehouse warehouse)
            {
                var editWindow = new AddEditWarehouseWindow(warehouse.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadWarehouses();
                }
            }
        }

        // --- دوال المواقع الفرعية ---
        private void AddLocationButton_Click(object sender, RoutedEventArgs e)
        {
            if (WarehousesDataGrid.SelectedItem is Warehouse selectedWarehouse)
            {
                var addWindow = new AddEditStorageLocationWindow(selectedWarehouse.Id);
                if (addWindow.ShowDialog() == true)
                {
                    LoadLocationsForWarehouse(selectedWarehouse.Id);
                }
            }
        }

        private void EditLocationButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is StorageLocation location)
            {
                var editWindow = new AddEditStorageLocationWindow(location.WarehouseId, location.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadLocationsForWarehouse(location.WarehouseId);
                }
            }
        }

        private void DeleteLocationButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is StorageLocation location)
            {
                // **مهم:** يجب إضافة تحقق هنا للتأكد من عدم وجود مخزون في هذا الموقع قبل الحذف
                var result = MessageBox.Show($"هل أنت متأكد من حذف الموقع '{location.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DatabaseContext())
                        {
                            var locationToDelete = db.StorageLocations.Find(location.Id);
                            if (locationToDelete != null)
                            {
                                db.StorageLocations.Remove(locationToDelete);
                                db.SaveChanges();
                                LoadLocationsForWarehouse(location.WarehouseId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"فشل حذف الموقع: {ex.InnerException?.Message ?? ex.Message}", "خطأ");
                    }
                }
            }
        }
    }
}
