// UI/Views/AddEditLeaveTypeWindow.xaml.cs
// *** الكود الخلفي المعدل ليعمل مع ViewModel ***
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditLeaveTypeWindow : Window
    {
        public AddEditLeaveTypeWindow()
        {
            InitializeComponent();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AddEditLeaveTypeViewModel viewModel)
            {
                if (viewModel.SaveCommand.CanExecute(null))
                {
                    if (viewModel.SaveCommand is AsyncRelayCommand asyncCommand)
                    {
                        await asyncCommand.ExecuteAsync();
                        this.DialogResult = true;
                    }
                }
            }
        }
    }
}