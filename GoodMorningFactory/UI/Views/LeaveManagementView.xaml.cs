// UI/Views/LeaveManagementView.xaml.cs
// *** ملف جديد: الكود الخلفي لواجهة إدارة الإجازات ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class LeaveManagementView : UserControl
    {
        public LeaveManagementView()
        {
            InitializeComponent();
            LoadLeaveRequests();
        }

        private void LoadLeaveRequests()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    LeaveRequestsDataGrid.ItemsSource = db.LeaveRequests
                        .Include(lr => lr.Employee)
                        .Include(lr => lr.LeaveType)
                        .OrderByDescending(lr => lr.StartDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل طلبات الإجازات: {ex.Message}");
            }
        }

        private void AddLeaveRequestButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddLeaveRequestWindow();
            if (addWindow.ShowDialog() == true) { LoadLeaveRequests(); }
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is LeaveRequest request)
            {
                UpdateLeaveStatus(request.Id, LeaveRequestStatus.Approved, "الموافقة على");
            }
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is LeaveRequest request)
            {
                UpdateLeaveStatus(request.Id, LeaveRequestStatus.Rejected, "رفض");
            }
        }

        private void UpdateLeaveStatus(int requestId, LeaveRequestStatus newStatus, string actionName)
        {
            var result = MessageBox.Show($"هل أنت متأكد من {actionName} هذا الطلب؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            using (var db = new DatabaseContext())
            {
                var request = db.LeaveRequests.Find(requestId);
                if (request != null)
                {
                    request.Status = newStatus;
                    db.SaveChanges();
                    LoadLeaveRequests();
                }
            }
        }
    }
}