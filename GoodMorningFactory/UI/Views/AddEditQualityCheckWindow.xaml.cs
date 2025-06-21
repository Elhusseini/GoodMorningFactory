// UI/Views/AddEditQualityCheckWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة عملية الفحص ***
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditQualityCheckWindow : Window
    {
        public AddEditQualityCheckWindow()
        {
            InitializeComponent();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddEditQualityCheckViewModel viewModel)
            {
                if (viewModel.SaveCommand is AsyncRelayCommand asyncCommand)
                {
                    await asyncCommand.ExecuteAsync();
                    DialogResult = true;
                }
            }
        }
    }
}