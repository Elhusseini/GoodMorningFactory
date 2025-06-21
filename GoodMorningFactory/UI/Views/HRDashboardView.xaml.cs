using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// Interaction logic for HRDashboardView.xaml
    /// </summary>
    public partial class HRDashboardView : UserControl
    {
        public HRDashboardView()
        {
            InitializeComponent();
            // تعيين الـ ViewModel ليكون هو مصدر البيانات لهذه الواجهة.
            DataContext = new HRDashboardViewModel();
        }
    }
}
