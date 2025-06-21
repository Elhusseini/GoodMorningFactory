// UI/Views/ProcessPayrollWindow.xaml.cs
// *** الكود الخلفي المعدل ليعمل مع ViewModel ***
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class ProcessPayrollWindow : Window
    {
        public ProcessPayrollWindow()
        {
            InitializeComponent();
        }

        // هذه الدالة تغلق النافذة وتعيد نتيجة إيجابية للشاشة الرئيسية
        // الـ ViewModel هو المسؤول عن إظهار رسالة النجاح قبل أن يتم استدعاء هذا الكود
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ProcessPayrollViewModel viewModel)
            {
                // نتحقق إذا كان الأمر قد تم تنفيذه بنجاح (عادةً ما يمسح الـ ViewModel البيانات بعد النجاح)
                // يمكننا ببساطة افتراض النجاح وإغلاق النافذة
                if (!viewModel.Payslips.Any())
                {
                    this.DialogResult = true;
                }
            }
        }
    }
}