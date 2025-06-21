using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class StockTransfersView : UserControl
    {
        public StockTransfersView()
        {
            InitializeComponent();
            DataContext = new StockTransfersViewModel();
        }
    }
}