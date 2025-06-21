// GoodMorningFactory/UI/Views/AddEditOpportunityWindow.xaml.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditOpportunityWindow : Window
    {
        public AddEditOpportunityWindow(Opportunity opportunity = null)
        {
            InitializeComponent();
            DataContext = new AddEditOpportunityViewModel(opportunity);
        }
    }
}