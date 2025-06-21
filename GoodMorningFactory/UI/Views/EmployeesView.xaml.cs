// UI/Views/EmployeesView.xaml.cs
// *** ملف جديد: الكود الخلفي لواجهة إدارة الموظفين ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class EmployeesView : UserControl
    {
        public EmployeesView()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // جلب الموظفين وتحويلهم إلى ViewModel للعرض
                    var employees = db.Employees
                        .Select(e => new
                        {
                            e.Id,
                            e.EmployeeCode,
                            FullName = e.FirstName + " " + e.LastName,
                            e.JobTitle,
                            e.Department,
                            e.Status
                        })
                        .ToList();
                    EmployeesDataGrid.ItemsSource = employees;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الموظفين: {ex.Message}");
            }
        }

        private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditEmployeeWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadEmployees();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // استخراج الهوية من الكائن المجهول
            if ((sender as Button)?.DataContext is object selectedItem)
            {
                var idProperty = selectedItem.GetType().GetProperty("Id");
                if (idProperty != null)
                {
                    int employeeId = (int)idProperty.GetValue(selectedItem, null);
                    var editWindow = new AddEditEmployeeWindow(employeeId);
                    if (editWindow.ShowDialog() == true)
                    {
                        LoadEmployees();
                    }
                }
            }
        }
    }
}