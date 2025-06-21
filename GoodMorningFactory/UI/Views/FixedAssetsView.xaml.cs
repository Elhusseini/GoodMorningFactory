// UI/Views/FixedAssetsView.xaml.cs
// *** ملف جديد: الكود الخلفي لواجهة إدارة الأصول الثابتة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class FixedAssetsView : UserControl
    {
        public FixedAssetsView()
        {
            InitializeComponent();
            LoadAssets();
        }

        private void LoadAssets()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    AssetsDataGrid.ItemsSource = db.FixedAssets.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل الأصول الثابتة: {ex.Message}", "خطأ");
            }
        }

        private void AddAssetButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditFixedAssetWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadAssets();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is FixedAsset asset)
            {
                var editWindow = new AddEditFixedAssetWindow(asset.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadAssets();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // (منطق الحذف الآمن يمكن إضافته هنا)
        }

        private void RunDepreciationButton_Click(object sender, RoutedEventArgs e)
        {
            var depreciationWindow = new RunDepreciationWindow();
            depreciationWindow.ShowDialog();
            // لا حاجة لتحديث الواجهة هنا لأنها لا تعرض الإهلاك
        }
    }
}