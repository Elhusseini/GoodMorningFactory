// UI/Views/AddManualAttendanceWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة التسجيل اليدوي ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddManualAttendanceWindow : Window
    {
        public AddManualAttendanceWindow()
        {
            InitializeComponent();
            LoadEmployees();
            AttendanceDatePicker.SelectedDate = DateTime.Today;
        }

        private void LoadEmployees()
        {
            using (var db = new DatabaseContext())
            {
                EmployeeComboBox.ItemsSource = db.Employees.ToList();
            }
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            SaveRecord(RecordType.In);
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            SaveRecord(RecordType.Out);
        }

        private void SaveRecord(RecordType type)
        {
            if (EmployeeComboBox.SelectedItem == null || AttendanceDatePicker.SelectedDate == null || !TimeSpan.TryParse(TimeTextBox.Text, out TimeSpan time))
            {
                MessageBox.Show("يرجى التأكد من اختيار الموظف وإدخال التاريخ والوقت بشكل صحيح (HH:mm).");
                return;
            }

            var selectedEmployee = (Employee)EmployeeComboBox.SelectedItem;
            var selectedDate = AttendanceDatePicker.SelectedDate.Value;
            var finalTimestamp = selectedDate.Date + time;

            try
            {
                using (var db = new DatabaseContext())
                {
                    db.AttendanceRecords.Add(new AttendanceRecord
                    {
                        EmployeeId = selectedEmployee.Id,
                        Timestamp = finalTimestamp,
                        RecordType = type
                    });
                    db.SaveChanges();
                }
                MessageBox.Show("تم تسجيل الحركة بنجاح.");
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تسجيل الحركة: {ex.Message}");
            }
        }
    }
}