// UI/ViewModels/EmployeesViewModel.cs
// *** ملف جديد: ViewModel لواجهة إدارة الموظفين ***

using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views; // لاستدعاء النافذة
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class EmployeesViewModel : BaseViewModel
    {
        private readonly IHRService _hrService;

        public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();
        public Employee SelectedEmployee { get; set; }

        public ICommand LoadEmployeesCommand { get; }
        public ICommand AddEmployeeCommand { get; }
        public ICommand EditEmployeeCommand { get; }

        public EmployeesViewModel()
        {
            _hrService = new HRService();

            LoadEmployeesCommand = new AsyncRelayCommand(LoadEmployeesAsync);
            AddEmployeeCommand = new RelayCommand(_ => AddEmployee());
            EditEmployeeCommand = new RelayCommand(async _ => await EditEmployee(), _ => SelectedEmployee != null);

            // تحميل الموظفين عند بدء تشغيل الـ ViewModel
            LoadEmployeesCommand.Execute(null);
        }

        private async Task LoadEmployeesAsync()
        {
            try
            {
                var employeesList = await _hrService.GetEmployeesAsync();
                Employees.Clear();
                foreach (var emp in employeesList)
                {
                    Employees.Add(emp);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل قائمة الموظفين: {ex.Message}", "خطأ");
            }
        }

        private void AddEmployee()
        {
            // إنشاء ViewModel جديد لنافذة الإضافة
            var addViewModel = new AddEditEmployeeViewModel(_hrService);
            var addWindow = new AddEditEmployeeWindow
            {
                DataContext = addViewModel // ربط النافذة بالـ ViewModel
            };

            // إذا تم الحفظ بنجاح، أعد تحميل القائمة
            if (addWindow.ShowDialog() == true)
            {
                LoadEmployeesCommand.Execute(null);
            }
        }

        private async Task EditEmployee()
        {
            if (SelectedEmployee == null) return;

            // نحتاج لجلب الكائن الكامل من قاعدة البيانات لضمان أننا نعدل على أحدث نسخة
            var employeeToEdit = await _hrService.GetEmployeeByIdAsync(SelectedEmployee.Id);
            if (employeeToEdit == null)
            {
                MessageBox.Show("لم يتم العثور على الموظف المحدد.", "خطأ");
                return;
            }

            // إنشاء ViewModel جديد لنافذة التعديل
            var editViewModel = new AddEditEmployeeViewModel(_hrService, employeeToEdit);
            var editWindow = new AddEditEmployeeWindow
            {
                DataContext = editViewModel // ربط النافذة بالـ ViewModel
            };

            if (editWindow.ShowDialog() == true)
            {
                LoadEmployeesCommand.Execute(null);
            }
        }
    }
}