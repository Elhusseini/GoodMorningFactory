// UI/Views/RecordLaborWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة تسجيل وقت العمالة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class RecordLaborWindow : Window
    {
        private readonly int _workOrderId;

        public RecordLaborWindow(int workOrderId)
        {
            InitializeComponent();
            _workOrderId = workOrderId;
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            using (var db = new DatabaseContext())
            {
                var workOrder = db.WorkOrders.Find(_workOrderId);
                if (workOrder != null)
                {
                    WorkOrderNumberTextBlock.Text = workOrder.WorkOrderNumber;
                }

                EmployeeComboBox.ItemsSource = db.Employees.Where(e => e.Status == EmployeeStatus.Active).ToList();
                WorkDatePicker.SelectedDate = DateTime.Today;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem == null ||
                WorkDatePicker.SelectedDate == null ||
                !decimal.TryParse(HoursWorkedTextBox.Text, out decimal hours) || hours <= 0)
            {
                MessageBox.Show("يرجى اختيار الموظف وإدخال تاريخ وعدد ساعات صحيح.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedEmployee = (Employee)EmployeeComboBox.SelectedItem;

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // تقدير تكلفة الساعة من الراتب الشهري (يمكن تعديل هذا المنطق لاحقًا)
                    // افتراض 22 يوم عمل في الشهر، 8 ساعات في اليوم
                    decimal hourlyRate = selectedEmployee.BasicSalary / (22 * 8);

                    var laborLog = new WorkOrderLaborLog
                    {
                        WorkOrderId = _workOrderId,
                        EmployeeId = selectedEmployee.Id,
                        Date = WorkDatePicker.SelectedDate.Value,
                        HoursWorked = hours,
                        HourlyRate = hourlyRate,
                        Description = DescriptionTextBox.Text
                    };

                    db.WorkOrderLaborLogs.Add(laborLog);

                    // تحديث التكلفة الإجمالية في أمر العمل
                    var workOrder = db.WorkOrders.Find(_workOrderId);
                    workOrder.TotalLaborCost += laborLog.TotalCost;

                    db.SaveChanges();
                    transaction.Commit();

                    MessageBox.Show("تم تسجيل وقت العمالة بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
