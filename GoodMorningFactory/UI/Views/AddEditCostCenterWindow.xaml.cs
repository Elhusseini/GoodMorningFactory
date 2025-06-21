// UI/Views/AddEditCostCenterWindow.xaml.cs
// *** الكود الكامل والمعدل - أصبح نظيفاً تماماً ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditCostCenterWindow : Window
    {
        public AddEditCostCenterWindow(int? costCenterId = null)
        {
            InitializeComponent();
            var service = new CostCenterService();
            DataContext = new AddEditCostCenterViewModel(service, costCenterId);
        }
    }
}