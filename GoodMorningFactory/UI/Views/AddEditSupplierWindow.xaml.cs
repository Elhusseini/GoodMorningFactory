// GoodMorningFactory/UI/Views/AddEditSupplierWindow.xaml.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditSupplierWindow : Window
    {
        private readonly AddEditSupplierViewModel _viewModel;

        // *** بداية التصحيح: تم التأكد من أن المُنشئ يقبل 'int?' ***
        public AddEditSupplierWindow(int? supplierId = null)
        {
            InitializeComponent();
            _viewModel = new AddEditSupplierViewModel(new SupplierService(), supplierId);
            DataContext = _viewModel;
        }
        // *** نهاية التصحيح ***

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (await _viewModel.SaveAsync())
            {
                DialogResult = true;
            }
        }
    }
}
