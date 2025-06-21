// UI/ViewModels/AttendanceViewModel.cs
// *** ملف جديد: ViewModel خاص بعرض سجلات الحضور المجمعة ***
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AttendanceViewModel
    {
        public string EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public string TimeIn { get; set; }
        public string TimeOut { get; set; }
        public string HoursWorked { get; set; }
        public string Status { get; set; }
    }
}