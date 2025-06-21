using GoodMorningFactory.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditWorkOrderWindow : Window
    {
        public AddEditWorkOrderWindow(int? workOrderId = null, int? salesOrderItemId = null)
        {
            InitializeComponent();
            DataContext = new AddEditWorkOrderViewModel(workOrderId, salesOrderItemId);
        }
    }
}