using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditWarehouseWindow : Window
    {
        public AddEditWarehouseWindow(int? warehouseId = null)
        {
            InitializeComponent();
            DataContext = new AddEditWarehouseViewModel(warehouseId);
        }
    }
}