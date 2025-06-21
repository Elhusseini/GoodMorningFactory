using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class RolesView : UserControl
    {
        public RolesView()
        {
            InitializeComponent();
            DataContext = new RolesViewViewModel();
        }
    }
}
