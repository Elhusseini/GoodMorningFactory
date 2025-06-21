// UI/ViewModels/AddEditLeaveTypeViewModel.cs
// *** ملف جديد: ViewModel لنافذة إضافة وتعديل نوع إجازة ***

using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditLeaveTypeViewModel : BaseViewModel
    {
        private readonly IHRService _hrService;

        private LeaveType _leaveType;
        public LeaveType LeaveType
        {
            get => _leaveType;
            set { _leaveType = value; OnPropertyChanged(); }
        }

        public string WindowTitle { get; private set; }

        public ICommand SaveCommand { get; }

        // المنشئ الخاص بحالة "الإضافة"
        public AddEditLeaveTypeViewModel(IHRService hrService)
        {
            _hrService = hrService;
            LeaveType = new LeaveType { IsPaid = true }; // القيمة الافتراضية
            WindowTitle = "إضافة نوع إجازة جديد";
            SaveCommand = new AsyncRelayCommand(SaveAsync);
        }

        // المنشئ الخاص بحالة "التعديل"
        public AddEditLeaveTypeViewModel(IHRService hrService, LeaveType leaveTypeToEdit)
        {
            _hrService = hrService;
            LeaveType = leaveTypeToEdit;
            WindowTitle = $"تعديل نوع إجازة: {LeaveType.Name}";
            SaveCommand = new AsyncRelayCommand(SaveAsync);
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(LeaveType.Name))
            {
                MessageBox.Show("اسم النوع حقل مطلوب.", "بيانات ناقصة");
                return;
            }

            try
            {
                await _hrService.SaveLeaveTypeAsync(LeaveType);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ نوع الإجازة: {ex.Message}", "خطأ");
            }
        }
    }
}