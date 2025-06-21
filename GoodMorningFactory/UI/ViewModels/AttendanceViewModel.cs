// UI/ViewModels/AttendanceViewModel.cs
// *** ملف جديد: ViewModel الرئيسي لشاشة الحضور والانصراف ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AttendanceViewModel : BaseViewModel
    {
        private readonly IHRService _hrService;

        public ObservableCollection<AttendanceSummaryViewModel> AttendanceSummary { get; set; } = new ObservableCollection<AttendanceSummaryViewModel>();

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged();
                    // تحديث القائمة تلقائياً عند تغيير التاريخ
                    LoadAttendanceCommand.Execute(null);
                }
            }
        }

        public ICommand LoadAttendanceCommand { get; }
        public ICommand AddManualAttendanceCommand { get; }

        public AttendanceViewModel()
        {
            _hrService = new HRService();
            LoadAttendanceCommand = new AsyncRelayCommand(LoadAttendanceAsync);
            AddManualAttendanceCommand = new RelayCommand(_ => AddManualAttendance());

            LoadAttendanceCommand.Execute(null); // تحميل بيانات اليوم الحالي عند بدء التشغيل
        }

        private async Task LoadAttendanceAsync()
        {
            var summary = await _hrService.GetAttendanceSummaryAsync(SelectedDate);
            AttendanceSummary.Clear();
            foreach (var item in summary)
            {
                AttendanceSummary.Add(item);
            }
        }

        private void AddManualAttendance()
        {
            var addViewModel = new AddManualAttendanceViewModel(_hrService);
            var addWindow = new AddManualAttendanceWindow { DataContext = addViewModel };
            // استبدلنا if بـ ShowDialog() مباشرة لأن ال ViewModel يعالج النجاح والفشل
            addWindow.ShowDialog();
            // بعد إغلاق النافذة، قم بتحديث السجلات
            LoadAttendanceCommand.Execute(null);
        }
    }
}