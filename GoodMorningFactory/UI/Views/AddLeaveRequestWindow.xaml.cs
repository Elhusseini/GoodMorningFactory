// UI/Views/AddLeaveRequestWindow.xaml.cs
// *** الكود الخلفي المعدل ليعمل مع ViewModel ***
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddLeaveRequestWindow : Window
    {
        public AddLeaveRequestWindow()
        {
            InitializeComponent();
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AddLeaveRequestViewModel viewModel)
            {
                if (viewModel.SubmitCommand is AsyncRelayCommand asyncCommand)
                {
                    await asyncCommand.ExecuteAsync();
                    // أغلق النافذة فقط إذا لم تكن هناك رسالة خطأ (بمعنى أن العملية نجحت)
                    // ال ViewModel سيتولى إظهار رسالة النجاح
                    this.DialogResult = true;
                }
            }
        }
    }
}