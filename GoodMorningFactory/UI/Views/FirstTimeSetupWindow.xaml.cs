// GoodMorning/UI/Views/FirstTimeSetupWindow.xaml.cs
// *** الكود الكامل والنهائي - أصبح نظيفاً ومساعداً للـ ViewModel ***
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class FirstTimeSetupWindow : Window
    {
        public FirstTimeSetupWindow()
        {
            InitializeComponent();
            PasswordBox.Focus();
        }

        private void CreateAdminButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FirstTimeSetupViewModel vm)
            {
                // تمرير صناديق كلمة المرور إلى الأمر في الـ ViewModel
                vm.CreateAdminCommand.Execute(new Tuple<PasswordBox, PasswordBox>(PasswordBox, ConfirmPasswordBox));
            }
        }
    }
}