using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// الكود الخلفي (Code-behind) لواجهة عرض فواتير المبيعات.
    /// بعد تطبيق نمط MVVM، أصبح هذا الملف بسيطًا جدًا.
    /// </summary>
    public partial class SalesView : UserControl
    {
        public SalesView()
        {
            InitializeComponent();
            // تعيين الـ DataContext إلى الـ ViewModel الجديد.
            // هذا هو الرابط بين الواجهة والمنطق البرمجي الخاص بها.
            this.DataContext = new SalesViewModel();
        }
    }
}
