// UI/ViewModels/AddLeaveRequestViewModel.cs
// *** ملف جديد: ViewModel لنافذة تقديم طلب إجازة ***

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
    public class AddLeaveRequestViewModel : BaseViewModel
    {
        private readonly IHRService _hrService;

        // الخصائص التي سيتم ربطها بالواجهة
        public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();
        public ObservableCollection<LeaveType> LeaveTypes { get; set; } = new ObservableCollection<LeaveType>();

        private LeaveRequest _leaveRequest;
        public LeaveRequest LeaveRequest
        {
            get => _leaveRequest;
            set { _leaveRequest = value; OnPropertyChanged(); }
        }

        public ICommand SubmitCommand { get; }

        public AddLeaveRequestViewModel(IHRService hrService)
        {
            _hrService = hrService;
            LeaveRequest = new LeaveRequest
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                Status = LeaveRequestStatus.Pending
            };

            SubmitCommand = new AsyncRelayCommand(SubmitAsync);

            // تحميل البيانات الأولية (قائمة الموظفين والاجازات)
            _ = LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                var employeesList = await _hrService.GetEmployeesAsync();
                Employees.Clear();
                foreach (var emp in employeesList) { Employees.Add(emp); }

                var leaveTypesList = await _hrService.GetLeaveTypesAsync();
                LeaveTypes.Clear();
                foreach (var type in leaveTypesList) { LeaveTypes.Add(type); }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        private async Task SubmitAsync()
        {
            if (LeaveRequest.EmployeeId == 0 || LeaveRequest.LeaveTypeId == 0)
            {
                MessageBox.Show("يرجى اختيار الموظف ونوع الإجازة.", "بيانات ناقصة");
                return;
            }

            try
            {
                await _hrService.AddLeaveRequestAsync(LeaveRequest);
                MessageBox.Show("تم تقديم الطلب بنجاح.", "نجاح");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تقديم الطلب: {ex.Message}", "خطأ");
            }
        }
    }
}