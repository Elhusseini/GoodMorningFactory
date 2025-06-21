// UI/Views/UnitsOfMeasureView.xaml.cs
// *** ملف جديد: الكود الخلفي لواجهة إدارة وحدات القياس ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class UnitsOfMeasureView : UserControl
    {
        public UnitsOfMeasureView()
        {
            InitializeComponent();
            LoadUoms();
        }

        private void LoadUoms()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    UomDataGrid.ItemsSource = db.UnitsOfMeasure.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل وحدات القياس: {ex.Message}");
            }
        }

        private void AddUomButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditUomWindow();
            if (addWindow.ShowDialog() == true) { LoadUoms(); }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is UnitOfMeasure uom)
            {
                var editWindow = new AddEditUomWindow(uom.Id);
                if (editWindow.ShowDialog() == true) { LoadUoms(); }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is UnitOfMeasure uom)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف وحدة القياس '{uom.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;

                try
                {
                    using (var db = new DatabaseContext())
                    {
                        // تحقق من عدم استخدام الوحدة في أي منتج
                        bool isUsed = db.Products.Any(p => p.UnitOfMeasureId == uom.Id);
                        if (isUsed)
                        {
                            MessageBox.Show("لا يمكن حذف وحدة القياس لأنها مستخدمة في منتجات.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        var entity = db.UnitsOfMeasure.Find(uom.Id);
                        if (entity != null)
                        {
                            db.UnitsOfMeasure.Remove(entity);
                            db.SaveChanges();
                            LoadUoms();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"حدث خطأ أثناء الحذف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}