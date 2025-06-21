using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// Interaction logic for CategoriesView.xaml
    /// </summary>
    public partial class CategoriesView : UserControl
    {
        public CategoriesView()
        {
            InitializeComponent();
            DataContext = new CategoriesViewViewModel();
        }
    }
}
