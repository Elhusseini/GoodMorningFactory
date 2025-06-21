// UI/Views/ReportsView.xaml.cs 
// *** الكود الكامل للكود الخلفي بعد تنظيفه *** using System.Windows.Controls;

using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// واجهة عرض التقارير.
    /// بعد تطبيق نمط MVVM، أصبح الكود الخلفي نظيفًا وخاليًا من أي منطق برمجي.
    /// دوره الآن يقتصر على تهيئة الواجهة فقط.
    /// </summary>
    public partial class ReportsView : UserControl
    {
        public ReportsView()
        {
            InitializeComponent();
            // الـ ViewModel الذي تم تعيينه في ملف XAML سيتولى كل شيء من الآن فصاعدًا.
        }
    }
}