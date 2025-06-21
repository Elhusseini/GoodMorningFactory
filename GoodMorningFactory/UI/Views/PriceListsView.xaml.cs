// UI/Views/PriceListsView.xaml.cs
// *** ملف جديد: الكود الخلفي لواجهة إدارة قوائم الأسعار ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class PriceListsView : UserControl
    {
        public PriceListsView()
        {
            InitializeComponent();
            LoadPriceLists();
        }

        private void LoadPriceLists()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    PriceListsDataGrid.ItemsSource = db.PriceLists.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل قوائم الأسعار: {ex.Message}");
            }
        }

        private void AddPriceListButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditPriceListWindow();
            if (addWindow.ShowDialog() == true) { LoadPriceLists(); }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PriceList priceList)
            {
                var editWindow = new AddEditPriceListWindow(priceList.Id);
                if (editWindow.ShowDialog() == true) { LoadPriceLists(); }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) { /* ... */ }
    }
}