// GoodMorningFactory/UI/ViewModels/SettingsViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GoodMorningFactory.UI.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;

        #region Properties

        private CompanyInfo _companySettings;
        public CompanyInfo CompanySettings
        {
            get => _companySettings;
            set { _companySettings = value; OnPropertyChanged(); }
        }

        private BitmapImage _logoImage;
        public BitmapImage LogoImage
        {
            get => _logoImage;
            set { _logoImage = value; OnPropertyChanged(); }
        }

        public ObservableCollection<object> LanguageOptions { get; }
        public ObservableCollection<object> DateFormatOptions { get; }
        public ObservableCollection<Currency> Currencies { get; set; }
        public ObservableCollection<Role> Roles { get; set; }
        public ObservableCollection<Account> Accounts { get; set; }
        public ObservableCollection<Account> ExpenseAccounts { get; set; }
        public ObservableCollection<Account> AssetAccounts { get; set; }
        public ObservableCollection<Account> LiabilityAccounts { get; set; }
        public ObservableCollection<object> CostingMethodOptions { get; set; }
        public ObservableCollection<NumberingSequence> NumberingSequences { get; set; }
        public ObservableCollection<NotificationSetting> NotificationSettings { get; set; }
        public ObservableCollection<BackupFileViewModel> BackupFiles { get; set; }

        private bool _isLightThemeChecked = true;
        public bool IsLightThemeChecked
        {
            get => _isLightThemeChecked;
            set { _isLightThemeChecked = value; OnPropertyChanged(); ChangeTheme(); }
        }

        #endregion

        #region Commands
        public ICommand LoadSettingsCommand { get; }
        public ICommand SaveCompanyInfoCommand { get; }
        public ICommand SaveGeneralSettingsCommand { get; }
        public ICommand SaveUserSettingsCommand { get; }
        public ICommand SaveDefaultAccountsCommand { get; }
        public ICommand SaveInventorySettingsCommand { get; }
        public ICommand SaveBackupSettingsCommand { get; }
        public ICommand SaveNumberingCommand { get; }
        public ICommand SaveNotificationsCommand { get; }
        public ICommand UploadLogoCommand { get; }
        public ICommand ManageCurrenciesCommand { get; }
        public ICommand CreateBackupCommand { get; }
        public ICommand RestoreBackupCommand { get; }
        public ICommand DeleteBackupCommand { get; }
        #endregion

        public SettingsViewModel()
        {
            _settingsService = new SettingsService();

            // تهيئة الخصائص والقوائم
            LanguageOptions = new ObservableCollection<object> { "العربية", "الإنجليزية" };
            DateFormatOptions = new ObservableCollection<object> { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
            CompanySettings = new CompanyInfo();
            Currencies = new ObservableCollection<Currency>();
            Roles = new ObservableCollection<Role>();
            Accounts = new ObservableCollection<Account>();
            ExpenseAccounts = new ObservableCollection<Account>();
            AssetAccounts = new ObservableCollection<Account>();
            LiabilityAccounts = new ObservableCollection<Account>();
            NumberingSequences = new ObservableCollection<NumberingSequence>();
            NotificationSettings = new ObservableCollection<NotificationSetting>();
            BackupFiles = new ObservableCollection<BackupFileViewModel>();

            // تهيئة الأوامر
            LoadSettingsCommand = new RelayCommand(async _ => await LoadSettingsAsync());
            SaveCompanyInfoCommand = new AsyncCommand(SaveCompanyInfoAsync, "حفظ معلومات المصنع");
            SaveGeneralSettingsCommand = new AsyncCommand(SaveGeneralSettingsAsync, "حفظ الإعدادات العامة");
            SaveUserSettingsCommand = new AsyncCommand(SaveUserSettingsAsync, "حفظ إعدادات المستخدمين");
            SaveDefaultAccountsCommand = new AsyncCommand(SaveDefaultAccountsAsync, "حفظ الحسابات الافتراضية");
            SaveInventorySettingsCommand = new AsyncCommand(SaveInventorySettingsAsync, "حفظ إعدادات المخزون");
            SaveBackupSettingsCommand = new AsyncCommand(SaveBackupSettingsAsync, "حفظ إعدادات النسخ الاحتياطي");
            SaveNumberingCommand = new AsyncCommand(SaveNumberingAsync, "حفظ إعدادات الترقيم");
            SaveNotificationsCommand = new AsyncCommand(SaveNotificationsAsync, "حفظ إعدادات الإشعارات");
            UploadLogoCommand = new RelayCommand(UploadLogo);
            ManageCurrenciesCommand = new RelayCommand(ManageCurrencies);
            CreateBackupCommand = new AsyncCommand(CreateBackupAsync, "إنشاء نسخة احتياطية");
            RestoreBackupCommand = new AsyncCommand<object>(RestoreBackupAsync, "استعادة نسخة");
            DeleteBackupCommand = new AsyncCommand<object>(DeleteBackupAsync, "حذف نسخة");
            // *** نهاية الإصلاح ***

            LoadSettingsCommand.Execute(null);
        }

        #region Loading Logic
        private async Task LoadSettingsAsync()
        {
            try
            {
                CompanySettings = await _settingsService.GetCompanyInfoAsync();
                DisplayLogo(CompanySettings.Logo);

                var allAccounts = await _settingsService.GetAccountsAsync();
                Accounts.Clear(); allAccounts.ForEach(a => Accounts.Add(a));
                ExpenseAccounts.Clear(); allAccounts.Where(a => a.AccountType == AccountType.Expense).ToList().ForEach(a => ExpenseAccounts.Add(a));
                AssetAccounts.Clear(); allAccounts.Where(a => a.AccountType == AccountType.Asset).ToList().ForEach(a => AssetAccounts.Add(a));
                LiabilityAccounts.Clear(); allAccounts.Where(a => a.AccountType == AccountType.Liability).ToList().ForEach(a => LiabilityAccounts.Add(a));

                var currencies = await _settingsService.GetActiveCurrenciesAsync();
                Currencies.Clear(); currencies.ForEach(c => Currencies.Add(c));
                // إعادة تعيين القيمة المختارة بعد تحميل المصدر لضمان عرضها بشكل صحيح
                OnPropertyChanged(nameof(Currencies));
                int? selectedCurrencyId = CompanySettings.DefaultCurrencyId;
                CompanySettings.DefaultCurrencyId = selectedCurrencyId;

                var roles = await _settingsService.GetRolesAsync();
                Roles.Clear(); roles.ForEach(r => Roles.Add(r));
                OnPropertyChanged(nameof(Roles));
                int? selectedRoleId = CompanySettings.DefaultRoleId;
                CompanySettings.DefaultRoleId = selectedRoleId;

                var costingMethods = Enum.GetValues(typeof(InventoryCostingMethod))
                    .Cast<InventoryCostingMethod>()
                    .Select(e => new { Value = e, Description = GetEnumDescription(e) });
                CostingMethodOptions = new ObservableCollection<object>(costingMethods);
                OnPropertyChanged(nameof(CostingMethodOptions));

                var numbering = await _settingsService.GetNumberingSequencesAsync();
                NumberingSequences.Clear(); numbering.ForEach(n => NumberingSequences.Add(n));

                var notifications = await _settingsService.GetNotificationSettingsAsync();
                NotificationSettings.Clear(); notifications.ForEach(n => NotificationSettings.Add(n));

                await LoadBackupsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ فادح أثناء تحميل الإعدادات: {ex.Message}", "خطأ تحميل", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadBackupsAsync()
        {
            var backups = await _settingsService.GetBackupFilesAsync();
            BackupFiles.Clear();
            foreach (var backup in backups) BackupFiles.Add(backup);
        }
        #endregion

        #region Saving Logic
        // دوال الحفظ تبقى كما هي
        private async Task SaveCompanyInfoAsync() => await _settingsService.SaveCompanyInfoAsync(CompanySettings);
        private async Task SaveGeneralSettingsAsync() => await _settingsService.SaveGeneralSettingsAsync(CompanySettings);
        private async Task SaveUserSettingsAsync() => await _settingsService.SaveUserSettingsAsync(CompanySettings);

        // --- بداية التعديل: التأكد من أن الدالة تستدعي الخدمة الصحيحة ---
        private async Task SaveDefaultAccountsAsync() => await _settingsService.SaveDefaultAccountsAsync(CompanySettings);
        // --- نهاية التعديل ---

        private async Task SaveInventorySettingsAsync() => await _settingsService.SaveInventorySettingsAsync(CompanySettings);
        private async Task SaveBackupSettingsAsync() => await _settingsService.SaveBackupSettingsAsync(CompanySettings);
        private async Task SaveNumberingAsync() => await _settingsService.SaveNumberingSequencesAsync(NumberingSequences);
        private async Task SaveNotificationsAsync() => await _settingsService.SaveNotificationSettingsAsync(NotificationSettings);
        #endregion

        #region Command Implementations
        private void UploadLogo(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "ملفات الصور (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg", Title = "اختر شعار الشركة" };
            if (openFileDialog.ShowDialog() == true)
            {
                var bytes = File.ReadAllBytes(openFileDialog.FileName);
                CompanySettings.Logo = bytes;
                OnPropertyChanged(nameof(CompanySettings));
                DisplayLogo(bytes);
            }
        }

        private async Task CreateBackupAsync()
        {
            await _settingsService.CreateBackupAsync();
            await LoadBackupsAsync();
        }

        private async Task RestoreBackupAsync(object parameter)
        {
            if (parameter is BackupFileViewModel backup)
            {
                var result = MessageBox.Show($"هل أنت متأكد من استعادة النسخة '{backup.FileName}'؟\nهذه العملية لا يمكن التراجع عنها وسوف تستبدل البيانات الحالية.", "تحذير خطير!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    await _settingsService.RestoreBackupAsync(backup.FilePath);
                    MessageBox.Show("تمت استعادة النسخة الاحتياطية بنجاح. يرجى إعادة تشغيل البرنامج لتطبيق التغييرات.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async Task DeleteBackupAsync(object parameter)
        {
            if (parameter is BackupFileViewModel backup)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف النسخة الاحتياطية '{backup.FileName}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    await _settingsService.DeleteBackupAsync(backup.FilePath);
                    await LoadBackupsAsync();
                }
            }
        }

        // --- بداية الإصلاح ---
        // تم استبدال الكود القديم بالكود الجديد الذي يستخدم خدمة التنقل المركزية
        private void ManageCurrencies(object parameter)
        {
            AppServices.NavigationService.NavigateTo("Currencies");
        }
        // --- نهاية الإصلاح ---
        #endregion

        #region Helper Methods
        private void DisplayLogo(byte[] logoBytes)
        {
            if (logoBytes != null && logoBytes.Length > 0)
            {
                BitmapImage image = new BitmapImage();
                using (MemoryStream stream = new MemoryStream(logoBytes))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                }
                LogoImage = image;
            }
            else
            {
                LogoImage = null;
            }
        }

        private void ChangeTheme()
        {
            try
            {
                string themeFile = IsLightThemeChecked ? "Themes/LightTheme.xaml" : "Themes/DarkTheme.xaml";
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri(themeFile, UriKind.Relative);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل المظهر: {ex.Message}", "خطأ");
            }
        }

        private static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute == null ? value.ToString() : attribute.Description;
        }
        #endregion
    }

    // *** بداية الإصلاح: تعديل كلاس الأوامر ليصبح أكثر مرونة ***

    /// <summary>
    /// كلاس مساعد لتنفيذ الأوامر غير المتزامنة التي لا تتطلب مُعامِل.
    /// </summary>
    public class AsyncCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly string _successMessage;
        private bool _isExecuting;

        public AsyncCommand(Func<Task> execute, string successMessage)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _successMessage = successMessage;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => !_isExecuting;

        public async void Execute(object parameter)
        {
            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();
            try
            {
                await _execute();
                if (!string.IsNullOrEmpty(_successMessage))
                    MessageBox.Show($"تم {_successMessage} بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تنفيذ الأمر: {ex.Message}\n{ex.InnerException?.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    /// <summary>
    /// كلاس مساعد لتنفيذ الأوامر غير المتزامنة التي تتطلب مُعامِل.
    /// </summary>
    public class AsyncCommand<T> : ICommand
    {
        private readonly Func<T, Task> _execute;
        private readonly string _successMessage;
        private bool _isExecuting;

        public AsyncCommand(Func<T, Task> execute, string successMessage)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _successMessage = successMessage;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => !_isExecuting;

        public async void Execute(object parameter)
        {
            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();
            try
            {
                await _execute((T)parameter);
                if (!string.IsNullOrEmpty(_successMessage))
                    MessageBox.Show($"تم {_successMessage} بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تنفيذ الأمر: {ex.Message}\n{ex.InnerException?.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
    // *** نهاية الإصلاح ***
}