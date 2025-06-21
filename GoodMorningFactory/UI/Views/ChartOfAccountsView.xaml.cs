using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class ChartOfAccountsView : UserControl
    {
        public ChartOfAccountsView()
        {
            InitializeComponent();
            // تعيين الـ ViewModel ليكون هو مصدر البيانات لهذه الواجهة.
            DataContext = new ChartOfAccountsViewModel();
        }
    }
}
