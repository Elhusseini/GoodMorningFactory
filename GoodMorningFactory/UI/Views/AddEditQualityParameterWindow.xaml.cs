// UI/Views/AddEditQualityParameterWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة إضافة وتعديل معيار فحص ***

using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// نافذة منبثقة لإضافة أو تعديل معيار فحص جودة.
    /// </summary>
    public partial class AddEditQualityParameterWindow : Window
    {
        public AddEditQualityParameterWindow()
        {
            InitializeComponent();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // نتأكد أن الـ ViewModel موجود ونجلب الأمر منه
            if (this.DataContext is AddEditQualityParameterViewModel viewModel)
            {
                if (viewModel.SaveCommand.CanExecute(null))
                {
                    // نستدعي الأمر وننتظره لينتهي من الحفظ
                    if (viewModel.SaveCommand is AsyncRelayCommand asyncCommand)
                    {
                        await asyncCommand.ExecuteAsync();
                        // بعد الحفظ الناجح، نغلق النافذة ونرجع بنتيجة إيجابية
                        this.DialogResult = true;
                    }
                }
            }
        }
    }
}