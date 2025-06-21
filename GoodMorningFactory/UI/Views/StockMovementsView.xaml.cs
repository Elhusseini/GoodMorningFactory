using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class StockMovementsView : UserControl
    {
        public StockMovementsView()
        {
            InitializeComponent();
            DataContext = new StockMovementsViewModel();
        }
    }
}