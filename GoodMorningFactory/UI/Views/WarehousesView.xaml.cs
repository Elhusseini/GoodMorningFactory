using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class WarehousesView : UserControl
    {
        public WarehousesView()
        {
            InitializeComponent();
            DataContext = new WarehousesViewModel();
        }
    }
}