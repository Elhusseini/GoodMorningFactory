using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class AccountsReceivableView : UserControl
    {
        public AccountsReceivableView()
        {
            InitializeComponent();
            DataContext = new AccountsReceivableViewViewModel();
        }
    }
}