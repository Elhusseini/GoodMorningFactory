// UI/Views/PurchaseRequisitionsView.xaml.cs
// *** الكود الكامل للكود الخلفي لواجهة طلبات الشراء مع جميع الوظائف ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
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
                if (requisition.Status != RequisitionStatus.Draft)
                {
                    MessageBox.Show("لا يمكن تعديل هذا الطلب إلا إذا كان في حالة 'مسودة'.", "عملية غير ممكنة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var editWindow = new AddEditPurchaseRequisitionWindow(requisition.Id);
                if (editWindow.ShowDialog() == true) { LoadRequisitions(); }
            }
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseRequisition requisition)
            {
                UpdateRequisitionStatus(requisition.Id, RequisitionStatus.Approved, "الموافقة على");
            }
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseRequisition requisition)
            {
                UpdateRequisitionStatus(requisition.Id, RequisitionStatus.Rejected, "رفض");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PurchaseRequisition requisition)
            {
                UpdateRequisitionStatus(requisition.Id, RequisitionStatus.Cancelled, "إلغاء");
            }
        }

        private void UpdateRequisitionStatus(int requisitionId, RequisitionStatus newStatus, string actionName)
        {
            var result = MessageBox.Show($"هل أنت متأكد من {actionName} هذا الطلب؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            using (var db = new DatabaseContext())
            {
                var requisition = db.PurchaseRequisitions.Find(requisitionId);
                if (requisition != null)
                {
                    requisition.Status = newStatus;
                    db.SaveChanges();
                    LoadRequisitions();
                }
            }
        }

        // --- بداية التحديث: تفعيل منطق تحويل الطلب ---
        private void ConvertToPOButton_Click(object sender, RoutedEventArgs e)
        {
            if (!PermissionsService.CanAccess("Purchases.Orders.Create"))
            {
                MessageBox.Show("ليس لديك صلاحية لإنشاء أوامر شراء.", "وصول مرفوض", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // --- نهاية التحديث ---
            if ((sender as Button)?.DataContext is PurchaseRequisition requisition)
            {
                if (requisition.Status != RequisitionStatus.Approved)
                {
                    MessageBox.Show("لا يمكن تحويل هذا الطلب إلى أمر شراء إلا إذا كان في حالة 'موافق عليه'.", "عملية غير ممكنة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // فتح نافذة أمر الشراء مع تمرير هوية طلب الشراء
                var poWindow = new AddEditPurchaseOrderWindow(sourceRequisitionId: requisition.Id);
                if (poWindow.ShowDialog() == true)
                {
                    // تحديث قائمة طلبات الشراء بعد إنشاء أمر الشراء
                    LoadRequisitions();
                }
            }
        }
        // --- نهاية التحديث ---
    }
}