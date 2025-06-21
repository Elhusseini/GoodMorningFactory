// GoodMorningFactory/UI/ViewModels/AddEditUserViewModel.cs
// *** الكود الكامل والنهائي بعد إصلاح الأخطاء البرمجية ***
using GoodMorningFactory.Core.Helpers;
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditUserViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        private readonly User _user;

        #region Properties
        private string _windowTitle;
        public string WindowTitle { get => _windowTitle; set { _windowTitle = value; OnPropertyChanged(); } }

        private User _userToSave;
        public User UserToSave { get => _userToSave; set { _userToSave = value; OnPropertyChanged(); } }

        private BitmapImage _profileImage;
        public BitmapImage ProfileImage { get => _profileImage; set { _profileImage = value; OnPropertyChanged(); } }

        public ObservableCollection<Role> Roles { get; } = new ObservableCollection<Role>();
        public ObservableCollection<Department> Departments { get; } = new ObservableCollection<Department>();

        private bool _isUsernameEnabled;
        public bool IsUsernameEnabled { get => _isUsernameEnabled; set { _isUsernameEnabled = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        public RelayCommand SaveCommand { get; }
        public RelayCommand UploadImageCommand { get; }
        #endregion

        public AddEditUserViewModel(User user = null)
        {
            _userService = new UserService();
            _user = user;

            SaveCommand = new RelayCommand(async (p) => await SaveAsync(p));
            UploadImageCommand = new RelayCommand(UploadImage); // الإصلاح هنا

            LoadInitialData();
        }

        private async void LoadInitialData()
        {
            try
            {
                var roles = await _userService.GetRolesAsync();
                var departments = await _userService.GetDepartmentsAsync();

                Roles.Clear();
                foreach (var role in roles) Roles.Add(role);

                Departments.Clear();
                Departments.Add(new Department { Id = 0, Name = "(بدون)" });
                foreach (var dept in departments) Departments.Add(dept);

                if (_user != null)
                {
                    WindowTitle = "تعديل مستخدم";
                    UserToSave = new User
                    {
                        Id = _user.Id,
                        Username = _user.Username,
                        FirstName = _user.FirstName,
                        LastName = _user.LastName,
                        Email = _user.Email,
                        PhoneNumber = _user.PhoneNumber,
                        RoleId = _user.RoleId,
                        DepartmentId = _user.DepartmentId,
                        IsActive = _user.IsActive,
                        ProfilePicture = _user.ProfilePicture,
                        CreatedAt = _user.CreatedAt
                    };
                    IsUsernameEnabled = false;
                }
                else
                {
                    WindowTitle = "إضافة مستخدم جديد";
                    UserToSave = new User { IsActive = true, CreatedAt = DateTime.Now };
                    IsUsernameEnabled = true;
                }
                DisplayImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات الأولية: {ex.Message}", "خطأ");
            }
        }

        private void UploadImage(object parameter) // الإصلاح هنا
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg" };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    UserToSave.ProfilePicture = File.ReadAllBytes(openFileDialog.FileName);
                    DisplayImage();
                }
                catch (Exception ex) { MessageBox.Show($"فشل تحميل الصورة: {ex.Message}", "خطأ"); }
            }
        }

        private void DisplayImage()
        {
            BitmapImage image = null;
            if (UserToSave.ProfilePicture != null && UserToSave.ProfilePicture.Length > 0)
            {
                image = new BitmapImage();
                using (var stream = new MemoryStream(UserToSave.ProfilePicture))
                {
                    stream.Position = 0;
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                }
            }
            else
            {
                try
                {
                    string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "default-user.png");
                    if (File.Exists(imagePath)) image = new BitmapImage(new Uri(imagePath));
                }
                catch { /* تجاهل الخطأ */ }
            }
            if (image != null) image.Freeze();
            ProfileImage = image;
        }

        private async Task SaveAsync(object parameter)
        {
            var passwordBoxes = parameter as Tuple<PasswordBox, PasswordBox>;
            if (passwordBoxes == null) return;

            var password = passwordBoxes.Item1.Password;
            var confirmPassword = passwordBoxes.Item2.Password;

            if (!await IsValid(password, confirmPassword)) return;

            try
            {
                if (UserToSave.DepartmentId == 0) UserToSave.DepartmentId = null;

                if (!string.IsNullOrWhiteSpace(password))
                {
                    UserToSave.PasswordHash = PasswordHelper.HashPassword(password);
                }

                if (UserToSave.Id == 0) { await _userService.AddUserAsync(UserToSave); }
                else { await _userService.UpdateUserAsync(UserToSave); }

                MessageBox.Show("تم حفظ المستخدم بنجاح.", "نجاح");
                var window = Application.Current.Windows.OfType<AddEditUserWindow>().FirstOrDefault();
                if (window != null) window.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ المستخدم: {ex.Message}", "خطأ");
            }
        }

        private async Task<bool> IsValid(string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(UserToSave.Username) || string.IsNullOrWhiteSpace(UserToSave.FirstName) ||
                string.IsNullOrWhiteSpace(UserToSave.LastName) || string.IsNullOrWhiteSpace(UserToSave.Email) ||
                UserToSave.RoleId == 0)
            {
                MessageBox.Show("يرجى ملء جميع الحقول الإلزامية (*).", "بيانات ناقصة");
                return false;
            }

            if (UserToSave.Id == 0 && string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("كلمة المرور مطلوبة عند إنشاء مستخدم جديد.", "بيانات ناقصة");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(password) && password != confirmPassword)
            {
                MessageBox.Show("كلمتا المرور غير متطابقتين.", "خطأ في الإدخال");
                return false;
            }

            if (await _userService.IsUsernameTakenAsync(UserToSave.Username, _user?.Id))
            {
                MessageBox.Show("اسم المستخدم هذا موجود بالفعل.", "بيانات مكررة");
                return false;
            }

            if (await _userService.IsEmailTakenAsync(UserToSave.Email, _user?.Id))
            {
                MessageBox.Show("هذا البريد الإلكتروني مستخدم بالفعل.", "بيانات مكررة");
                return false;
            }

            return true;
        }
    }
}