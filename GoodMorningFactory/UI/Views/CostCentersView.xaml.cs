// UI/Views/CostCentersView.xaml.cs
// *** ملف جديد: الكود الخلفي لواجهة إدارة مراكز التكلفة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class CostCentersView : UserControl
    {
        public CostCentersView()
        {
            InitializeComponent();
            LoadCostCenters();
        }

        private void LoadCostCenters()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    CostCentersDataGrid.ItemsSource = db.CostCenters.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل مراكز التكلفة: {ex.Message}", "خطأ");
            }
        }

        private void AddCostCenterButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditCostCenterWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadCostCenters();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is CostCenter center)
            {
                var editWindow = new AddEditCostCenterWindow(center.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadCostCenters();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // (منطق الحذف الآمن يمكن إضافته هنا)
        }
    }
}