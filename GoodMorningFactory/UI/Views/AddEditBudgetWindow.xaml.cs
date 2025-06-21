// UI/Views/AddEditBudgetWindow.xaml.cs
// *** الكود الكامل والمعدل - أصبح نظيفاً تماماً ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditBudgetWindow : Window
    {
        public AddEditBudgetWindow(int? budgetId = null)
        {
            InitializeComponent();
            var service = new BudgetService();
            DataContext = new AddEditBudgetViewModel(service, budgetId);
        }
    }
}