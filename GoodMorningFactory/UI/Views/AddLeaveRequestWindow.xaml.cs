// UI/Views/AddLeaveRequestWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة تقديم طلب إجازة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddLeaveRequestWindow : Window
    {
        public AddLeaveRequestWindow()
        {
            InitializeComponent();
            LoadInitialData();
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today.AddDays(1);
        }

        private void LoadInitialData()
        {
            using (var db = new DatabaseContext())
            {
                EmployeeComboBox.ItemsSource = db.Employees.ToList();
                LeaveTypeComboBox.ItemsSource = db.LeaveTypes.ToList();
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem == null || LeaveTypeComboBox.SelectedItem == null ||
                StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("يرجى ملء جميع الحقول.", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            {
                db.LeaveRequests.Add(new LeaveRequest
                {
                    EmployeeId = ((Employee)EmployeeComboBox.SelectedItem).Id,
                    LeaveTypeId = (int)LeaveTypeComboBox.SelectedValue,
                    StartDate = StartDatePicker.SelectedDate.Value,
                    EndDate = EndDatePicker.SelectedDate.Value,
                    Status = LeaveRequestStatus.Pending
                });
                db.SaveChanges();
            }
            MessageBox.Show("تم تقديم طلب الإجازة بنجاح.", "نجاح");
            this.DialogResult = true;
        }
    }
}