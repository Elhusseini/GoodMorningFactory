using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class ManagePermissionsWindow : Window
    {
        public ManagePermissionsWindow(int roleId, bool isReadOnly = false)
        {
            InitializeComponent();
            DataContext = new ManagePermissionsViewModel(roleId, isReadOnly);
        }

        // The Save button in the ViewModel doesn't close the window, so we handle it here.
        private async void SavePermissions_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ManagePermissionsViewModel vm)
            {
                var saveCommand = vm.SaveCommand;
                if (saveCommand.CanExecute(null))
                {
                    saveCommand.Execute(null);
                    // A simple way to wait for async command to finish before closing
                    await System.Threading.Tasks.Task.Delay(500);
                    this.DialogResult = true;
                }
            }
        }
    }
}
