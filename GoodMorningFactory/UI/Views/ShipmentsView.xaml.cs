using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class ShipmentsView : UserControl
    {
        public ShipmentsView()
        {
            InitializeComponent();
            DataContext = new ShipmentsViewViewModel();
        }
    }
}
