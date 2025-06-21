// GoodMorningFactory/UI/Views/AddEditUserWindow.xaml.cs
// *** الكود الكامل والنهائي - أصبح نظيفاً ومساعداً للـ ViewModel ***
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditUserWindow : Window
    {
        public AddEditUserWindow(User user = null)
        {
            InitializeComponent();
            DataContext = new AddEditUserViewModel(user);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddEditUserViewModel vm)
            {
                // تمرير صناديق كلمة المرور إلى الأمر في الـ ViewModel
                vm.SaveCommand.Execute(new Tuple<PasswordBox, PasswordBox>(PasswordBox, ConfirmPasswordBox));
            }
        }
    }
}