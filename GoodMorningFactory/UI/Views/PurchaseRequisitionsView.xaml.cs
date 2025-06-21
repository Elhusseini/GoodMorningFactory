// UI/Views/PurchaseRequisitionsView.xaml.cs
// *** تحديث: تم إصلاح خطأ الترتيب حسب حقل decimal في SQLite ***
using GoodMorningFactory.Core.Services;
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
    public partial class PurchaseRequisitionsView : UserControl
    {
        private int _currentPage = 1;
        private int _pageSize = 15;
        private int _totalItems = 0;

        public PurchaseRequisitionsView()
        {
            InitializeComponent();

            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(RequisitionStatus)).Cast<object>());
            StatusFilterComboBox.ItemsSource = statuses;
            StatusFilterComboBox.SelectedIndex = 0;

            LoadRequisitions();
        }

        private void LoadRequisitions()
        {
            try
            {
                string searchText = SearchTextBox.Text.ToLower();

                using (var db = new DatabaseContext())
                {
                    var query = db.PurchaseRequisitions.AsQueryable();

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(pr => pr.RequisitionNumber.ToLower().Contains(searchText) || pr.RequesterName.ToLower().Contains(searchText));
                    }
                    if (StatusFilterComboBox.SelectedItem != null && StatusFilterComboBox.SelectedItem is RequisitionStatus status)
                    {
                        query = query.Where(pr => pr.Status == status);
                    }

                    _totalItems = query.Count();
                    RequisitionsDataGrid.ItemsSource = query.OrderByDescending(pr => pr.RequisitionDate).Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل طلبات الشراء: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePageInfo()
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (totalPages == 0) totalPages = 1;
            PageInfoTextBlock.Text = $"الصفحة {_currentPage} من {totalPages} (إجمالي السجلات: {_totalItems})";
            PreviousPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < totalPages;
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage > 1) { _currentPage--; LoadRequisitions(); } }
        private void NextPageButton_Click(object sender, RoutedEventArgs e) { if (_currentPage < (int)Math.Ceiling((double)_totalItems / _pageSize)) { _currentPage++; LoadRequisitions(); } }
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) { if (this.IsLoaded) { _currentPage = 1; LoadRequisitions(); } }
        private void Filter_KeyUp(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { _currentPage = 1; LoadRequisitions(); } }

        private void AddRequisitionButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditPurchaseRequisitionWindow();
            if (addWindow.ShowDialog() == true) { LoadRequisitions(); }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseRequisition requisition)
            {
                var editWindow = new AddEditPurchaseRequisitionWindow(requisition.Id);
                if (editWindow.ShowDialog() == true) { LoadRequisitions(); }
            }
        }

        private void SubmitForApprovalButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseRequisition requisition)
            {
                using (var db = new DatabaseContext())
                {
                    // --- بداية الإصلاح ---
                    // جلب البيانات إلى الذاكرة أولاً باستخدام ToList() ثم ترتيبها
                    var workflow = db.ApprovalWorkflows
                        .Include(w => w.Steps)
                        .Where(w => w.DocumentType == DocumentType.PurchaseRequisition && w.IsActive)
                        .ToList() // جلب البيانات إلى الذاكرة
                        .OrderByDescending(w => w.MinimumAmount) // الترتيب يتم الآن في ذاكرة التطبيق
                        .FirstOrDefault();
                    // --- نهاية الإصلاح ---

                    if (workflow != null && workflow.Steps.Any())
                    {
                        var approvalRequest = new ApprovalRequest
                        {
                            DocumentId = requisition.Id,
                            DocumentType = DocumentType.PurchaseRequisition,
                            RequestDate = DateTime.UtcNow,
                            Status = ApprovalStatus.Pending,
                            CurrentStepId = workflow.Steps.OrderBy(s => s.StepOrder).First().Id
                        };
                        db.ApprovalRequests.Add(approvalRequest);

                        var prToUpdate = db.PurchaseRequisitions.Find(requisition.Id);
                        prToUpdate.Status = RequisitionStatus.PendingApproval;
                        prToUpdate.ApprovalRequest = approvalRequest;
                    }
                    else
                    {
                        var prToUpdate = db.PurchaseRequisitions.Find(requisition.Id);
                        prToUpdate.Status = RequisitionStatus.Approved;
                    }

                    db.SaveChanges();
                    LoadRequisitions();
                    MessageBox.Show("تم إرسال الطلب للموافقة بنجاح.", "نجاح");
                }
            }
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateRequisitionStatus(sender, RequisitionStatus.Approved, "الموافقة على");
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateRequisitionStatus(sender, RequisitionStatus.Rejected, "رفض");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateRequisitionStatus(sender, RequisitionStatus.Cancelled, "إلغاء");
        }

        private void UpdateRequisitionStatus(object sender, RequisitionStatus newStatus, string actionName)
        {
            if ((sender as Button)?.DataContext is PurchaseRequisition requisition)
            {
                var result = MessageBox.Show($"هل أنت متأكد من {actionName} هذا الطلب؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;

                using (var db = new DatabaseContext())
                {
                    var reqInDb = db.PurchaseRequisitions.Find(requisition.Id);
                    if (reqInDb != null)
                    {
                        reqInDb.Status = newStatus;
                        db.SaveChanges();
                        LoadRequisitions();
                    }
                }
            }
        }

        private void ConvertToPOButton_Click(object sender, RoutedEventArgs e)
        {
            if (!PermissionsService.CanAccess("Purchasing.Orders.Create"))
            {
                MessageBox.Show("ليس لديك صلاحية لإنشاء أوامر شراء.", "وصول مرفوض", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if ((sender as Button)?.DataContext is PurchaseRequisition requisition)
            {
                var poWindow = new AddEditPurchaseOrderWindow(sourceRequisitionId: requisition.Id);
                if (poWindow.ShowDialog() == true)
                {
                    LoadRequisitions();
                }
            }
        }
    }
}
