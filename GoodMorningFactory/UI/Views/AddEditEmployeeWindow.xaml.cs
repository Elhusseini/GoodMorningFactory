// UI/Views/AddEditEmployeeWindow.xaml.cs
// *** الكود الكامل للكود الخلفي لنافذة إضافة وتعديل موظف ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditEmployeeWindow : Window
    {
        private int? _employeeId;
        private Employee _employeeToEdit;

        public AddEditEmployeeWindow(int? employeeId = null)
        {
            InitializeComponent();
            _employeeId = employeeId;
            GenderComboBox.ItemsSource = Enum.GetValues(typeof(Gender));

            if (_employeeId.HasValue)
            {
                LoadEmployeeData();
            }
            else
            {
                Title = "إضافة موظف جديد";
                EmployeeCodeTextBox.Text = $"EMP-{DateTime.Now:yyyyMMddHHmmss}";
                HireDatePicker.SelectedDate = DateTime.Today;
            }
        }

        private void LoadEmployeeData()
        {
            using (var db = new DatabaseContext())
            {
                _employeeToEdit = db.Employees.Find(_employeeId.Value);
                if (_employeeToEdit != null)
                {
                    Title = $"تعديل ملف الموظف: {_employeeToEdit.FirstName} {_employeeToEdit.LastName}";
                    EmployeeCodeTextBox.Text = _employeeToEdit.EmployeeCode;
                    FirstNameTextBox.Text = _employeeToEdit.FirstName;
                    LastNameTextBox.Text = _employeeToEdit.LastName;
                    DobDatePicker.SelectedDate = _employeeToEdit.DateOfBirth;
                    GenderComboBox.SelectedItem = _employeeToEdit.Gender;
                    NationalityTextBox.Text = _employeeToEdit.Nationality;
                    HireDatePicker.SelectedDate = _employeeToEdit.HireDate;
                    JobTitleTextBox.Text = _employeeToEdit.JobTitle;
                    DepartmentTextBox.Text = _employeeToEdit.Department;
                    BasicSalaryTextBox.Text = _employeeToEdit.BasicSalary.ToString();
                    HousingAllowanceTextBox.Text = _employeeToEdit.HousingAllowance.ToString();
                    TransportAllowanceTextBox.Text = _employeeToEdit.TransportationAllowance.ToString();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmployeeCodeTextBox.Text) || string.IsNullOrWhiteSpace(FirstNameTextBox.Text) || string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                MessageBox.Show("الرجاء ملء الحقول المطلوبة (كود الموظف، الاسم الأول، واسم العائلة).", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            {
                if (_employeeToEdit == null)
                {
                    _employeeToEdit = new Employee();
                    db.Employees.Add(_employeeToEdit);
                }
                else
                {
                    db.Employees.Attach(_employeeToEdit);
                }

                _employeeToEdit.EmployeeCode = EmployeeCodeTextBox.Text;
                _employeeToEdit.FirstName = FirstNameTextBox.Text;
                _employeeToEdit.LastName = LastNameTextBox.Text;
                _employeeToEdit.DateOfBirth = DobDatePicker.SelectedDate;
                if (GenderComboBox.SelectedItem != null) _employeeToEdit.Gender = (Gender)GenderComboBox.SelectedItem;
                _employeeToEdit.Nationality = NationalityTextBox.Text;
                _employeeToEdit.HireDate = HireDatePicker.SelectedDate ?? DateTime.Today;
                _employeeToEdit.JobTitle = JobTitleTextBox.Text;
                _employeeToEdit.Department = DepartmentTextBox.Text;

                decimal.TryParse(BasicSalaryTextBox.Text, out decimal basicSalary);
                _employeeToEdit.BasicSalary = basicSalary;
                decimal.TryParse(HousingAllowanceTextBox.Text, out decimal housing);
                _employeeToEdit.HousingAllowance = housing;
                decimal.TryParse(TransportAllowanceTextBox.Text, out decimal transport);
                _employeeToEdit.TransportationAllowance = transport;

                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}