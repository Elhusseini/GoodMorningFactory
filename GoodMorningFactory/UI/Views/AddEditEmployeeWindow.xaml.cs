// UI/Views/AddEditEmployeeWindow.xaml.cs
// *** الكود الكامل والمصحح للكود الخلفي لنافذة الإضافة والتعديل ***

using GoodMorningFactory.UI.Commands; // **مهم** لإيجاد AsyncRelayCommand
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditEmployeeWindow : Window
    {
        public AddEditEmployeeWindow()
        {
            InitializeComponent();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // التأكد من أن DataContext هو النوع الصحيح
            if (this.DataContext is AddEditEmployeeViewModel viewModel)
            {
                // التأكد من أن الأمر يمكن تنفيذه
                if (viewModel.SaveCommand.CanExecute(null))
                {
                    // *** تصحيح: استدعاء ExecuteAsync بدلاً من Execute ***
                    // نقوم بتحويل نوع الأمر إلى AsyncRelayCommand لنتمكن من الوصول للدالة الجديدة
                    if (viewModel.SaveCommand is AsyncRelayCommand asyncCommand)
                    {
                        await asyncCommand.ExecuteAsync();
                        // إذا نجح الحفظ (لم تحدث أخطاء)، أغلق النافذة بنجاح
                        this.DialogResult = true;
                    }
                }
            }
        }
    }
}