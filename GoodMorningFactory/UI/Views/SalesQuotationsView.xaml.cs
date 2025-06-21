// GoodMorningFactory/UI/Views/SalesQuotationsView.xaml.cs
// *** تحديث شامل: تم إزالة كل المنطق البرمجي من هنا ونقله إلى الـ ViewModel ***
using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// أصبح الكود الخلفي لهذه الواجهة بسيطًا جدًا بعد تطبيق نمط MVVM.
    /// دوره الآن يقتصر على تهيئة الواجهة (InitializeComponent) وربطها بالـ ViewModel.
    /// </summary>
    public partial class SalesQuotationsView : UserControl
    {
        public SalesQuotationsView()
        {
            InitializeComponent();

            // تعيين الـ ViewModel كمصدر بيانات لهذه الواجهة
            // كل منطق العرض والأوامر يتم التعامل معه الآن في SalesQuotationsViewModel
            DataContext = new SalesQuotationsViewModel();
        }
    }
}