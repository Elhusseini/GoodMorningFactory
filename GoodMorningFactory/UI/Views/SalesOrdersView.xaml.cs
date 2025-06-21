using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class SalesOrdersView : UserControl
    {
        public SalesOrdersView()
        {
            InitializeComponent();
            DataContext = new SalesOrdersViewModel();
        }
    }
}
