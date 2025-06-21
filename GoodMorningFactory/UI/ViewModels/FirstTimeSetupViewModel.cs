// GoodMorningFactory/UI/ViewModels/FirstTimeSetupViewModel.cs
// *** ملف جديد: ViewModel لنافذة الإعداد الأولي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.ViewModels
{
    public class FirstTimeSetupViewModel : BaseViewModel
    {
        private readonly ISetupService _setupService;

        public RelayCommand CreateAdminCommand { get; }

        public FirstTimeSetupViewModel()
        {
            _setupService = new SetupService();
            CreateAdminCommand = new RelayCommand(async (p) => await CreateAdminAsync(p));
        }

        private async Task CreateAdminAsync(object parameter)
        {
            var passwordBoxes = parameter as Tuple<PasswordBox, PasswordBox>;
            if (passwordBoxes == null) return;

            var password = passwordBoxes.Item1.Password;
            var confirmPassword = passwordBoxes.Item2.Password;

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("يرجى إدخال كلمة المرور.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (password != confirmPassword)
            {
                MessageBox.Show("كلمتا المرور غير متطابقتين.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                await _setupService.CreateAdminUserAsync(password);
                MessageBox.Show("تم إنشاء حساب المدير العام بنجاح. سيتم الآن الانتقال إلى شاشة تسجيل الدخول.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                // إغلاق النافذة من خلال الكود الخلفي
                var window = Application.Current.Windows.OfType<FirstTimeSetupWindow>().FirstOrDefault();
                if (window != null)
                {
                    window.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ غير متوقع أثناء إنشاء حساب المدير: {ex.Message}", "خطأ فادح", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}