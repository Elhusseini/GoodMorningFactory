// UI/Views/WorkOrdersView.xaml.cs
// *** تحديث: إضافة منطق إلغاء حجز المواد عند إلغاء أمر العمل ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public partial class WorkOrdersView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 15;
        private int _totalItems = 0;

        public WorkOrdersView()
        {
            InitializeComponent();
            LoadFilters();
            LoadWorkOrders();
        }

        private void LoadFilters()
        {
            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(WorkOrderStatus)).Cast<object>());
            StatusFilterComboBox.ItemsSource = statuses;
            StatusFilterComboBox.SelectedIndex = 0;
        }

        private void LoadWorkOrders()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.WorkOrders.Include(wo => wo.FinishedGood).AsQueryable();
                    string searchText = SearchTextBox.Text.ToLower();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(wo => wo.WorkOrderNumber.ToLower().Contains(searchText) || wo.FinishedGood.Name.ToLower().Contains(searchText));
                    }
                    if (StatusFilterComboBox.SelectedItem != null && StatusFilterComboBox.SelectedItem is WorkOrderStatus status)
                    {
                        query = query.Where(wo => wo.Status == status);
                    }
                    _totalItems = query.Count();
                    WorkOrdersDataGrid.ItemsSource = query.OrderByDescending(wo => wo.PlannedStartDate).Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
                    UpdatePageInfo();
                }
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء تحميل أوامر العمل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void UpdatePageInfo()
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (totalPages == 0) totalPages = 1;
            PageInfoTextBlock.Text = $"الصفحة {_currentPage} من {totalPages} (إجمالي الأوامر: {_totalItems})";
            PreviousPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < totalPages;
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage > 1) { _currentPage--; LoadWorkOrders(); } }
        private void NextPageButton_Click(object sender, RoutedEventArgs e) { int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize); if (_currentPage < totalPages) { _currentPage++; LoadWorkOrders(); } }
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) { if (this.IsLoaded) { _currentPage = 1; LoadWorkOrders(); } }
        private void Filter_KeyUp(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { _currentPage = 1; LoadWorkOrders(); } }
        private void AddWorkOrderButton_Click(object sender, RoutedEventArgs e) { var addWindow = new AddEditWorkOrderWindow(); if (addWindow.ShowDialog() == true) { LoadWorkOrders(); } }
        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e) { if ((sender as Button)?.DataContext is WorkOrder order) { var editWindow = new AddEditWorkOrderWindow(order.Id); if (editWindow.ShowDialog() == true) { LoadWorkOrders(); } } }
        private void StartButton_Click(object sender, RoutedEventArgs e) { if ((sender as Button)?.DataContext is WorkOrder order) { UpdateWorkOrderStatus(order.Id, WorkOrderStatus.InProgress, "بدء التنفيذ"); } }
        private void ConsumeMaterialsButton_Click(object sender, RoutedEventArgs e) { if ((sender as Button)?.DataContext is WorkOrder order) { var consumptionWindow = new MaterialConsumptionWindow(order.Id); consumptionWindow.ShowDialog(); } }
        private void ReportProductionButton_Click(object sender, RoutedEventArgs e) { if ((sender as Button)?.DataContext is WorkOrder order) { var productionWindow = new ReportProductionWindow(order.Id); if (productionWindow.ShowDialog() == true) { LoadWorkOrders(); } } }
        private void CancelButton_Click(object sender, RoutedEventArgs e) { if ((sender as Button)?.DataContext is WorkOrder order) { UpdateWorkOrderStatus(order.Id, WorkOrderStatus.Cancelled, "إلغاء"); } }

        // أضف هذه الدالة الجديدة إلى ملف WorkOrdersView.xaml.cs

        private void RecordLaborButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is WorkOrder order)
            {
                var laborWindow = new RecordLaborWindow(order.Id);
                laborWindow.ShowDialog();
                // لا نحتاج لتحديث القائمة هنا لأن التكلفة لا تظهر في الجدول الرئيسي
            }
        }

        private async void UpdateWorkOrderStatus(int orderId, WorkOrderStatus newStatus, string actionName)
        {
            if (newStatus == WorkOrderStatus.Cancelled)
            {
                var result = MessageBox.Show($"هل أنت متأكد من {actionName} أمر العمل؟ سيتم إلغاء حجز أي مواد متبقية.", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }

            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = await db.WorkOrders.FindAsync(orderId);
                    if (order != null && order.Status != WorkOrderStatus.Completed && order.Status != WorkOrderStatus.Cancelled)
                    {
                        // *** بداية التحديث: منطق إلغاء الحجز ***
                        if (newStatus == WorkOrderStatus.Cancelled)
                        {
                            var bom = await db.BillOfMaterials.Include(b => b.BillOfMaterialsItems).FirstOrDefaultAsync(b => b.FinishedGoodId == order.FinishedGoodId);
                            var consumed = await db.WorkOrderMaterials.Where(m => m.WorkOrderId == orderId).ToListAsync();

                            if (bom != null)
                            {
                                foreach (var bomItem in bom.BillOfMaterialsItems)
                                {
                                    decimal totalPlanned = bomItem.Quantity * order.QuantityToProduce;
                                    decimal totalConsumed = consumed.Where(c => c.RawMaterialId == bomItem.RawMaterialId).Sum(c => c.QuantityConsumed);
                                    int remainingReservation = (int)Math.Ceiling(totalPlanned - totalConsumed);

                                    if (remainingReservation > 0)
                                    {
                                        var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == bomItem.RawMaterialId);
                                        if (inventory != null)
                                        {
                                            inventory.QuantityReserved -= remainingReservation;
                                        }
                                    }
                                }
                            }
                        }
                        // *** نهاية التحديث ***

                        order.Status = newStatus;
                        if (newStatus == WorkOrderStatus.InProgress) order.ActualStartDate = DateTime.Now;
                        if (newStatus == WorkOrderStatus.Completed) order.ActualEndDate = DateTime.Now;

                        await db.SaveChangesAsync();
                        await transaction.CommitAsync();
                        LoadWorkOrders();
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    MessageBox.Show($"فشلت عملية {actionName}: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
