using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// Interaction logic for FinancialDashboardView.xaml
    /// </summary>
    public partial class FinancialDashboardView : UserControl
    {
        public FinancialDashboardView()
        {
            InitializeComponent();
            // تعيين الـ ViewModel ليكون هو مصدر البيانات لهذه الواجهة.
            // كل منطق الحسابات والعرض أصبح الآن داخل الـ ViewModel.
            DataContext = new FinancialDashboardViewModel();
        }
    }
}
