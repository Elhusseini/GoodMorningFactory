// UI/Views/AddManualAttendanceWindow.xaml.cs
// *** الكود الخلفي المعدل ***
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddManualAttendanceWindow : Window
    {
        public AddManualAttendanceWindow()
        {
            InitializeComponent();
        }

        // هذه الدالة تغلق النافذة بعد تنفيذ الأمر بنجاح من الـ ViewModel
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // تم تعديل هذا الجزء. لا نغلق النافذة تلقائياً
            // الـ ViewModel هو المسؤول عن إظهار رسالة النجاح
            // يمكن للمستخدم إغلاق النافذة يدوياً بعد رؤية الرسالة
            // أو يمكننا تعديل ال ViewModel ليغلق النافذة برمجياً (خطوة متقدمة)
            // حالياً، سنبقيها هكذا لتبسيط الأمور.
            this.DialogResult = true; // نرجع بنتيجة إيجابية لإعادة تحميل البيانات في الشاشة الرئيسية
        }
    }
}