using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditStorageLocationWindow : Window
    {
        public AddEditStorageLocationWindow(int warehouseId, int? locationId = null)
        {
            InitializeComponent();
            DataContext = new AddEditStorageLocationViewModel(warehouseId, locationId);
        }
    }
}