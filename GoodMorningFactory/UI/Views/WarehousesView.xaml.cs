// UI/Views/WarehousesView.xaml.cs
// *** ملف جديد: الكود الخلفي لواجهة إدارة المخازن ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
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
                    WarehousesDataGrid.ItemsSource = db.Warehouses.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المخازن: {ex.Message}");
            }
        }

        private void AddWarehouseButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditWarehouseWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadWarehouses();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // (منطق الحذف يمكن إضافته هنا مع التحقق من عدم وجود مخزون في المخزن)
        }
    }
}