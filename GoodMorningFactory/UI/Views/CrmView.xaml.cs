// UI/Views/CrmView.xaml.cs
// الكود الخلفي أصبح نظيفاً وبسيطاً جداً
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class CrmView : UserControl
    {
        public CrmView()
        {
            InitializeComponent();
            // الـ ViewModel يتم إنشاؤه الآن من خلال الـ XAML
            // لا حاجة لكتابة أي كود هنا حالياً
        }
    }
}