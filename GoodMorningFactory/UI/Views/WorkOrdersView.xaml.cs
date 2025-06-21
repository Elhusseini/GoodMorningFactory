using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class WorkOrdersView : UserControl
    {
        public WorkOrdersView()
        {
            InitializeComponent();
            DataContext = new WorkOrdersViewModel();
        }
    }
}