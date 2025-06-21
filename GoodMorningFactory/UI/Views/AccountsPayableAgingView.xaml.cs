using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class AccountsPayableAgingView : UserControl
    {
        public AccountsPayableAgingView()
        {
            InitializeComponent();
            DataContext = new AccountsPayableAgingViewModel();
        }
    }
}