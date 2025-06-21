// UI/Views/SalesOrdersView.xaml.cs
// *** تحديث: تم تعديل الكود لاستخدام ViewModel وعرض الحالات المنفصلة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels; // <-- إضافة مهمة
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public partial class SalesOrdersView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 15;
        private int _totalItems = 0;

        public SalesOrdersView()
        {
            InitializeComponent();
            LoadFilters();
            LoadOrders();
        }

        private void LoadFilters()
        {
            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(OrderStatus)).Cast<object>());
            StatusFilterComboBox.ItemsSource = statuses;
            StatusFilterComboBox.SelectedIndex = 0;
        }

        private void LoadOrders()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.SalesOrders.Include(o => o.Customer).AsQueryable();

                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(o => o.SalesOrderNumber.ToLower().Contains(searchText) || o.Customer.CustomerName.ToLower().Contains(searchText));
                    }

                    if (StatusFilterComboBox.SelectedItem != null && StatusFilterComboBox.SelectedItem is OrderStatus status)
                    {
                        query = query.Where(o => o.Status == status);
                    }

                    _totalItems = query.Count();

                    var ordersForPage = query.OrderByDescending(o => o.OrderDate)
                                             .Skip((_currentPage - 1) * _pageSize)
                                             .Take(_pageSize)
                                             .ToList();

                    // --- بداية التحديث: تحويل الأوامر إلى ViewModel ---
                    var viewModels = new List<SalesOrderViewModel>();
                    foreach (var order in ordersForPage)
                    {
                        viewModels.Add(new SalesOrderViewModel { Order = order });
                    }
                    OrdersDataGrid.ItemsSource = viewModels;
                    // --- نهاية التحديث ---

                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل أوامر البيع: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePageInfo()
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (totalPages == 0) totalPages = 1;
            PageInfoTextBlock.Text = $"الصفحة {_currentPage} من {totalPages} (إجمالي الأوامر: {_totalItems})";
            PreviousPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < totalPages;
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1) { _currentPage--; LoadOrders(); }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (_currentPage < totalPages) { _currentPage++; LoadOrders(); }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded) { _currentPage = 1; LoadOrders(); }
        }

        private void Filter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { _currentPage = 1; LoadOrders(); }
        }

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditSalesOrderWindow();
            if (addWindow.ShowDialog() == true) { LoadOrders(); }
        }

        // --- بداية التحديث: تعديل الدوال للتعامل مع ViewModel ---
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesOrderViewModel vm)
            {
                var editWindow = new AddEditSalesOrderWindow(vm.Order.Id);
                if (editWindow.ShowDialog() == true) { LoadOrders(); }
            }
        }

        private void CreateWorkOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesOrderViewModel vm)
            {
                var order = vm.Order;
                var result = MessageBox.Show($"سيتم الآن إنشاء أوامر عمل للمنتجات النهائية في أمر البيع رقم {order.SalesOrderNumber}.\nهل تريد المتابعة؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.No) return;

                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var orderItems = db.SalesOrderItems.Include(i => i.Product).Where(i => i.SalesOrderId == order.Id).ToList();
                        int createdCount = 0;
                        foreach (var item in orderItems)
                        {
                            if (item.Product.ProductType == ProductType.FinishedGood)
                            {
                                var workOrderWindow = new AddEditWorkOrderWindow(salesOrderItemId: item.Id);
                                if (workOrderWindow.ShowDialog() == true) { createdCount++; }
                            }
                        }
                        if (createdCount > 0)
                        {
                            var orderToUpdate = db.SalesOrders.Find(order.Id);
                            if (orderToUpdate != null) { orderToUpdate.Status = OrderStatus.InProcess; db.SaveChanges(); }
                            MessageBox.Show($"تم إنشاء {createdCount} أمر/أوامر عمل بنجاح. تم تغيير حالة أمر البيع إلى 'قيد التنفيذ'.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadOrders();
                        }
                        else
                        {
                            MessageBox.Show("أمر البيع هذا لا يحتوي على منتجات نهائية لإنشاء أوامر عمل لها.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show($"فشلت عملية إنشاء أمر العمل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void CreateShipmentButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesOrderViewModel vm)
            {
                var order = vm.Order;
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

        private void CreateInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesOrderViewModel vm)
            {
                var order = vm.Order;
                if (order.Status == OrderStatus.Invoiced || order.Status == OrderStatus.Cancelled)
                {
                    MessageBox.Show("لا يمكن إنشاء فاتورة لهذا الأمر.", "عملية غير ممكنة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var shipmentAndInvoiceWindow = new AddShipmentWindow(order.Id);
                if (shipmentAndInvoiceWindow.ShowDialog() == true)
                {
                    LoadOrders();
                }
            }
        }

        private void CancelOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is SalesOrderViewModel vm)
            {
                var orderToCancel = vm.Order;
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
        // --- نهاية التحديث ---
    }
}
