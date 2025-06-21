// UI/Views/PriceListsView.xaml.cs
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

        // === بداية التحديث: إضافة دالة لفتح نافذة إدارة الأسعار ===
        private void ManagePricesButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PriceList priceList)
            {
                var managePricesWindow = new ManageProductPricesWindow(priceList.Id);
                managePricesWindow.ShowDialog();
                // لا نحتاج لتحديث القائمة هنا لأن التغييرات تتم في نافذة أخرى
            }
        }
        // === نهاية التحديث ===

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PriceList priceList)
            {
                // يمكنك إضافة منطق التحقق من عدم وجود أسعار مرتبطة قبل الحذف
                var result = MessageBox.Show($"هل أنت متأكد من حذف قائمة الأسعار '{priceList.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var db = new DatabaseContext())
                    {
                        var entity = db.PriceLists.Find(priceList.Id);
                        if (entity != null)
                        {
                            db.PriceLists.Remove(entity);
                            db.SaveChanges();
                            LoadPriceLists();
                        }
                    }
                }
            }
        }
    }
}
