// UI/ViewModels/AddManualAttendanceViewModel.cs
// *** ملف جديد: ViewModel لنافذة التسجيل اليدوي للحضور ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddManualAttendanceViewModel : BaseViewModel
    {
        private readonly IHRService _hrService;

        public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();

        private Employee _selectedEmployee;
        public Employee SelectedEmployee { get => _selectedEmployee; set { _selectedEmployee = value; OnPropertyChanged(); } }

        private DateTime _attendanceDate = DateTime.Today;
        public DateTime AttendanceDate { get => _attendanceDate; set { _attendanceDate = value; OnPropertyChanged(); } }

        private string _attendanceTime = DateTime.Now.ToString("HH:mm");
        public string AttendanceTime { get => _attendanceTime; set { _attendanceTime = value; OnPropertyChanged(); } }

        public ICommand SignInCommand { get; }
        public ICommand SignOutCommand { get; }

        public AddManualAttendanceViewModel(IHRService hrService)
        {
            _hrService = hrService;
            SignInCommand = new AsyncRelayCommand(() => SaveRecordAsync(RecordType.In));
            SignOutCommand = new AsyncRelayCommand(() => SaveRecordAsync(RecordType.Out));
            _ = LoadEmployeesAsync();
        }

        private async Task LoadEmployeesAsync()
        {
            var employeesList = await _hrService.GetEmployeesAsync();
            Employees.Clear();
            foreach (var emp in employeesList) { Employees.Add(emp); }
        }

        private async Task SaveRecordAsync(RecordType type)
        {
            if (SelectedEmployee == null || !TimeSpan.TryParse(AttendanceTime, out var time))
            {
                MessageBox.Show("يرجى التأكد من اختيار الموظف وإدخال الوقت بشكل صحيح (HH:mm).");
                return;
            }

            var record = new AttendanceRecord
            {
                EmployeeId = SelectedEmployee.Id,
                Timestamp = AttendanceDate.Date + time,
                RecordType = type,
                Notes = "تسجيل يدوي"
            };

            await _hrService.AddAttendanceRecordAsync(record);
            MessageBox.Show("تم تسجيل الحركة بنجاح.", "نجاح");
        }
    }
}