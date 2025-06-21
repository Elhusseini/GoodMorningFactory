// UI/Views/AddEditSaleWindow.xaml.cs
// *** الكود الكامل والنهائي - أصبح نظيفاً ويدعم الإضافة والتعديل ***
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditSaleWindow : Window
    {
        public AddEditSaleWindow(int? saleId = null)
        {
            InitializeComponent();
            DataContext = new AddEditSaleViewModel(saleId);
        }
    }
}