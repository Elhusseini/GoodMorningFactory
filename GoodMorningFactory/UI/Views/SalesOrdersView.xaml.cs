// UI/Views/SalesOrdersView.xaml.cs
// *** الكود الكامل لواجهة أوامر البيع مع إضافة منطق الإلغاء ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class SalesOrdersView : UserControl
    {
        public SalesOrdersView()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    OrdersDataGrid.ItemsSource = db.SalesOrders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل أوامر البيع: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditSalesOrderWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadOrders();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesOrder order)
            {
                var editWindow = new AddEditSalesOrderWindow(order.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadOrders();
                }
            }
        }

        private void CreateShipmentButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesOrder order)
            {
                if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Invoiced || order.Status == OrderStatus.Cancelled)
                {
                    MessageBox.Show("لا يمكن إنشاء شحنة لهذا الأمر.", "عملية غير ممكنة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var shipmentWindow = new AddShipmentWindow(order.Id);
                if (shipmentWindow.ShowDialog() == true)
                {
                    LoadOrders();
                }
            }
        }

        // *** دالة جديدة لإلغاء أمر البيع ***
        private void CancelOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesOrder orderToCancel)
            {
                var result = MessageBox.Show($"هل أنت متأكد من إلغاء أمر البيع رقم '{orderToCancel.SalesOrderNumber}'؟", "تأكيد الإلغاء", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var order = db.SalesOrders.Find(orderToCancel.Id);
                        if (order != null)
                        {
                            order.Status = OrderStatus.Cancelled;
                            db.SaveChanges();
                            LoadOrders();
                            MessageBox.Show("تم إلغاء أمر البيع بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشلت عملية الإلغاء: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}