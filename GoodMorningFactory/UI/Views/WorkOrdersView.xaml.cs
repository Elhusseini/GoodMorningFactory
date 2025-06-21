// UI/Views/WorkOrdersView.xaml.cs
// الكود الكامل للكود الخلفي لواجهة أوامر العمل
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class WorkOrdersView : UserControl
    {
        public WorkOrdersView()
        {
            InitializeComponent();
            LoadWorkOrders();
        }

        private void LoadWorkOrders()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    WorkOrdersDataGrid.ItemsSource = db.WorkOrders.Include(wo => wo.FinishedGood).OrderByDescending(wo => wo.PlannedStartDate).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل أوامر العمل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddWorkOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditWorkOrderWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadWorkOrders();
            }
        }

        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is WorkOrder order)
            {
                var editWindow = new AddEditWorkOrderWindow(order.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadWorkOrders();
                }
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is WorkOrder order)
            {
                UpdateWorkOrderStatus(order.Id, WorkOrderStatus.InProgress, "بدء التنفيذ");
            }
        }

        private void ConsumeMaterialsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is WorkOrder order)
            {
                var consumptionWindow = new MaterialConsumptionWindow(order.Id);
                consumptionWindow.ShowDialog();
            }
        }

        private void ReportProductionButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is WorkOrder order)
            {
                var productionWindow = new ReportProductionWindow(order.Id);
                if (productionWindow.ShowDialog() == true)
                {
                    LoadWorkOrders();
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is WorkOrder order)
            {
                UpdateWorkOrderStatus(order.Id, WorkOrderStatus.Cancelled, "إلغاء");
            }
        }

        private void UpdateWorkOrderStatus(int orderId, WorkOrderStatus newStatus, string actionName)
        {
            var result = MessageBox.Show($"هل أنت متأكد من {actionName} أمر العمل؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            try
            {
                using (var db = new DatabaseContext())
                {
                    var order = db.WorkOrders.Find(orderId);
                    if (order != null)
                    {
                        order.Status = newStatus;
                        if (newStatus == WorkOrderStatus.InProgress) order.ActualStartDate = DateTime.Now;
                        if (newStatus == WorkOrderStatus.Completed) order.ActualEndDate = DateTime.Now;

                        db.SaveChanges();
                        LoadWorkOrders();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية {actionName}: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}