// UI/Views/AttendanceView.xaml.cs
// *** ملف جديد: الكود الخلفي لواجهة إدارة الحضور والانصراف ***
using GoodMorningFactory.Data;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class AttendanceView : UserControl
    {
        public AttendanceView()
        {
            InitializeComponent();
            DatePickerFilter.SelectedDate = DateTime.Today;
            LoadAttendance();
        }

        private void LoadAttendance()
        {
            if (DatePickerFilter.SelectedDate == null) return;
            DateTime selectedDate = DatePickerFilter.SelectedDate.Value.Date;

            try
            {
                using (var db = new DatabaseContext())
                {
                    var records = db.AttendanceRecords
                        .Include(r => r.Employee)
                        .Where(r => r.Timestamp.Date == selectedDate)
                        .ToList();

                    var groupedRecords = records
                        .GroupBy(r => r.Employee)
                        .Select(g =>
                        {
                            var timeIn = g.Where(r => r.RecordType == Data.Models.RecordType.In).Min(r => (DateTime?)r.Timestamp);
                            var timeOut = g.Where(r => r.RecordType == Data.Models.RecordType.Out).Max(r => (DateTime?)r.Timestamp);
                            var hoursWorked = (timeOut.HasValue && timeIn.HasValue) ? (timeOut.Value - timeIn.Value).TotalHours : 0;

                            return new AttendanceViewModel
                            {
                                EmployeeName = $"{g.Key.FirstName} {g.Key.LastName}",
                                Date = selectedDate,
                                TimeIn = timeIn?.ToString("T") ?? "N/A",
                                TimeOut = timeOut?.ToString("T") ?? "N/A",
                                HoursWorked = hoursWorked.ToString("F2"),
                                Status = (timeIn.HasValue && timeOut.HasValue) ? "حاضر" : (timeIn.HasValue ? "لم يسجل انصراف" : "غائب")
                            };
                        })
                        .ToList();

                    AttendanceDataGrid.ItemsSource = groupedRecords;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل سجلات الحضور: {ex.Message}", "خطأ");
            }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded) { LoadAttendance(); }
        }

        private void AddManualAttendance_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddManualAttendanceWindow();
            if (addWindow.ShowDialog() == true) { LoadAttendance(); }
        }
    }
}