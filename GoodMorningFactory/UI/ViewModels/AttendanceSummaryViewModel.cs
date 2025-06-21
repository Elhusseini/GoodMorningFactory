// UI/ViewModels/AttendanceSummaryViewModel.cs
// *** تحديث: تم تغيير اسم الملف وال namespace ليعكس وظيفته بشكل أفضل ***
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// يمثل هذا الكلاس نموذج عرض (DTO) لسطر واحد في جدول ملخص الحضور.
    /// لا يحتوي على منطق، فقط خصائص لعرض البيانات المجمعة.
    /// </summary>
    public class AttendanceSummaryViewModel
    {
        public string EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public string TimeIn { get; set; }
        public string TimeOut { get; set; }
        public string HoursWorked { get; set; }
        public string Status { get; set; }
    }
}