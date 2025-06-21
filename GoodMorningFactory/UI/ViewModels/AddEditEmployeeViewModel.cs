// UI/ViewModels/AddEditEmployeeViewModel.cs
// *** ملف جديد: ViewModel لنافذة إضافة وتعديل موظف ***

using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditEmployeeViewModel : BaseViewModel
    {
        private readonly IHRService _hrService;
        private Employee _employee;

        public Employee Employee
        {
            get => _employee;
            set { _employee = value; OnPropertyChanged(); }
        }

        public string WindowTitle { get; private set; }
        public List<Gender> Genders { get; } = Enum.GetValues(typeof(Gender)).Cast<Gender>().ToList();

        public ICommand SaveCommand { get; }

        // المنشئ الخاص بحالة "الإضافة"
        public AddEditEmployeeViewModel(IHRService hrService)
        {
            _hrService = hrService;
            Employee = new Employee
            {
                EmployeeCode = $"EMP-{DateTime.Now:yyyyMMddHHmmss}",
                HireDate = DateTime.Today,
                Status = EmployeeStatus.Active
            };
            WindowTitle = "إضافة موظف جديد";
            SaveCommand = new AsyncRelayCommand(SaveAsync);
        }

        // المنشئ الخاص بحالة "التعديل"
        public AddEditEmployeeViewModel(IHRService hrService, Employee employeeToEdit)
        {
            _hrService = hrService;
            Employee = employeeToEdit;
            WindowTitle = $"تعديل بيانات: {Employee.FirstName} {Employee.LastName}";
            SaveCommand = new AsyncRelayCommand(SaveAsync);
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Employee.EmployeeCode) || string.IsNullOrWhiteSpace(Employee.FirstName) || string.IsNullOrWhiteSpace(Employee.LastName))
            {
                MessageBox.Show("الرجاء ملء الحقول المطلوبة (كود الموظف، الاسم الأول، واسم العائلة).", "بيانات ناقصة");
                return;
            }

            try
            {
                await _hrService.SaveEmployeeAsync(Employee);
                // هنا يمكننا إغلاق النافذة، لكننا سنترك التحكم للـ View
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ الموظف: {ex.Message}", "خطأ");
            }
        }
    }
}