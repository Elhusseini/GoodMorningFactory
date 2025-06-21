// UI/Views/SettingsView.xaml.cs
using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// واجهة المستخدم لعرض وتعديل إعدادات التطبيق.
    /// هذه الواجهة الآن مرتبطة بشكل كامل بـ SettingsViewModel الذي يحتوي على كل المنطق.
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();

            // تعيين الـ DataContext إلى الـ ViewModel الجديد.
            // كل عمليات الربط في ملف الـ XAML ستعمل الآن مع هذا الكائن.
            DataContext = new SettingsViewModel();
        }
    }
}