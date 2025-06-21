// GoodMorningFactory/UI/Views/SupplierStatementWindow.xaml.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// Code-behind لنافذة عرض كشف حساب المورد.
    /// </summary>
    public partial class SupplierStatementWindow : Window
    {
        private readonly SupplierStatementViewModel _viewModel;

        public SupplierStatementWindow(int supplierId)
        {
            InitializeComponent();
            _viewModel = new SupplierStatementViewModel(new SupplierService(), supplierId);
            DataContext = _viewModel;
            Loaded += async (s, e) => await _viewModel.LoadStatementAsync();
        }
    }
}
