// UI/Views/AddSalesReturnWindow.xaml.cs
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddSalesReturnWindow : Window
    {
        public AddSalesReturnWindow(int saleId)
        {
            InitializeComponent();
            DataContext = new AddSalesReturnViewModel(saleId);
        }
    }
}