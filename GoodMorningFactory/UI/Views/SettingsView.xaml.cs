// UI/Views/SettingsView.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GoodMorningFactory.UI.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly string _appDataFolder;
        private readonly string _dbPath;
        private readonly string _backupFolder;
        private CompanyInfo? _companyInfo;
        private byte[]? _logoBytes;

        public SettingsView()
        {
            InitializeComponent();
            _appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GoodMorningFactory");
            _dbPath = Path.Combine(_appDataFolder, "GoodMorningFactory.db");
            _backupFolder = Path.Combine(_appDataFolder, "Backups");

            Directory.CreateDirectory(_backupFolder);

            LoadCompanyInfo();
            LoadGeneralSettingsOptions();
            LoadUserRoleSettingsOptions();
            LoadNumberingSequences();
            LoadNotificationSettings();
            LoadBackups();
            LoadDefaultAccounts();
        }

        private void LoadCompanyInfo()
        {
            using (var db = new DatabaseContext())
            {
                _companyInfo = db.CompanyInfos.FirstOrDefault();
                if (_companyInfo == null) { _companyInfo = new CompanyInfo { Id = 1 }; }

                // معلومات المصنع
                CompanyNameTextBox.Text = _companyInfo.CompanyName;
                AddressTextBox.Text = _companyInfo.Address;
                CityTextBox.Text = _companyInfo.City;
                CountryTextBox.Text = _companyInfo.Country;
                PhoneNumberTextBox.Text = _companyInfo.PhoneNumber;
                EmailTextBox.Text = _companyInfo.Email;
                WebsiteTextBox.Text = _companyInfo.Website;
                TaxNumberTextBox.Text = _companyInfo.TaxNumber;
                CommercialRegTextBox.Text = _companyInfo.CommercialRegistrationNumber;
                _logoBytes = _companyInfo.Logo;
                DisplayLogo();

                // الإعدادات العامة
                LanguageComboBox.SelectedItem = _companyInfo.DefaultLanguage ?? "العربية";
                DateFormatComboBox.SelectedItem = _companyInfo.DefaultDateFormat ?? "dd/MM/yyyy";
                CurrencyComboBox.SelectedItem = _companyInfo.BaseCurrency ?? "KWD";

                // إعدادات المستخدمين
                MinPassLengthTextBox.Text = _companyInfo.MinPasswordLength.ToString();
                PassExpiryTextBox.Text = _companyInfo.PasswordExpiryDays.ToString();
                LockoutAttemptsTextBox.Text = _companyInfo.FailedLoginLockoutAttempts.ToString();
                RequireUppercaseCheckBox.IsChecked = _companyInfo.RequireUppercase;
                RequireLowercaseCheckBox.IsChecked = _companyInfo.RequireLowercase;
                RequireDigitCheckBox.IsChecked = _companyInfo.RequireDigit;
                RequireSpecialCharCheckBox.IsChecked = _companyInfo.RequireSpecialChar;
                DefaultRoleComboBox.SelectedValue = _companyInfo.DefaultRoleId;
            }
        }

        private void LoadGeneralSettingsOptions()
        {
            LanguageComboBox.ItemsSource = new[] { "العربية", "English" };
            DateFormatComboBox.ItemsSource = new[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
            CurrencyComboBox.ItemsSource = new[] { "KWD", "USD", "EUR", "SAR" };
        }

        private void LoadUserRoleSettingsOptions()
        {
            using (var db = new DatabaseContext())
            {
                DefaultRoleComboBox.ItemsSource = db.Roles.ToList();
            }
        }

        private void LoadNumberingSequences()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    NumberingDataGrid.ItemsSource = db.NumberingSequences.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل إعدادات الترقيم: {ex.Message}", "خطأ");
            }
        }

        private void LoadNotificationSettings()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    NotificationsDataGrid.ItemsSource = db.NotificationSettings.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل إعدادات الإشعارات: {ex.Message}", "خطأ");
            }
        }

        private void LoadDefaultAccounts()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var accounts = db.Accounts.ToList();
                    SalesAccountComboBox.ItemsSource = accounts;
                    AccountsReceivableComboBox.ItemsSource = accounts;
                    PurchasesAccountComboBox.ItemsSource = accounts;
                    AccountsPayableComboBox.ItemsSource = accounts;

                    if (_companyInfo != null)
                    {
                        SalesAccountComboBox.SelectedValue = _companyInfo.DefaultSalesAccountId;
                        AccountsReceivableComboBox.SelectedValue = _companyInfo.DefaultAccountsReceivableId;
                        PurchasesAccountComboBox.SelectedValue = _companyInfo.DefaultPurchasesAccountId;
                        AccountsPayableComboBox.SelectedValue = _companyInfo.DefaultAccountsPayableId;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل الحسابات الافتراضية: {ex.Message}", "خطأ");
            }
        }

        private void LoadBackups()
        {
            try
            {
                var backupFiles = Directory.GetFiles(_backupFolder, "*.db")
                    .Select(path => new FileInfo(path))
                    .Select(fi => new BackupFileViewModel
                    {
                        FileName = fi.Name,
                        FilePath = fi.FullName,
                        CreationDate = fi.CreationTime,
                        FileSize = $"{fi.Length / 1024} KB"
                    })
                    .OrderByDescending(f => f.CreationDate)
                    .ToList();

                BackupsListView.ItemsSource = backupFiles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل قائمة النسخ الاحتياطية: {ex.Message}", "خطأ");
            }
        }

        private void SaveCompanyInfoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    if (_companyInfo == null) return;
                    
                    _companyInfo.CompanyName = CompanyNameTextBox.Text;
                    _companyInfo.Address = AddressTextBox.Text;
                    _companyInfo.City = CityTextBox.Text;
                    _companyInfo.Country = CountryTextBox.Text;
                    _companyInfo.PhoneNumber = PhoneNumberTextBox.Text;
                    _companyInfo.Email = EmailTextBox.Text;
                    _companyInfo.Website = WebsiteTextBox.Text;
                    _companyInfo.TaxNumber = TaxNumberTextBox.Text;
                    _companyInfo.CommercialRegistrationNumber = CommercialRegTextBox.Text;
                    _companyInfo.Logo = _logoBytes;

                    if (db.CompanyInfos.Any())
                    {
                        db.CompanyInfos.Update(_companyInfo);
                    }
                    else
                    {
                        db.CompanyInfos.Add(_companyInfo);
                    }

                    db.SaveChanges();
                    MessageBox.Show("تم حفظ معلومات المصنع بنجاح", "نجاح");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ معلومات المصنع: {ex.Message}", "خطأ");
            }
        }

        private void SaveGeneralSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    if (_companyInfo == null) return;

                    _companyInfo.DefaultLanguage = LanguageComboBox.SelectedItem?.ToString();
                    _companyInfo.DefaultDateFormat = DateFormatComboBox.SelectedItem?.ToString();
                    _companyInfo.BaseCurrency = CurrencyComboBox.SelectedItem?.ToString();

                    db.CompanyInfos.Update(_companyInfo);
                    db.SaveChanges();
                    MessageBox.Show("تم حفظ الإعدادات العامة بنجاح", "نجاح");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ الإعدادات العامة: {ex.Message}", "خطأ");
            }
        }

        private void SaveUserSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    if (_companyInfo == null) return;

                    if (int.TryParse(MinPassLengthTextBox.Text, out int minLength))
                        _companyInfo.MinPasswordLength = minLength;
                    
                    if (int.TryParse(PassExpiryTextBox.Text, out int expiryDays))
                        _companyInfo.PasswordExpiryDays = expiryDays;
                    
                    if (int.TryParse(LockoutAttemptsTextBox.Text, out int lockoutAttempts))
                        _companyInfo.FailedLoginLockoutAttempts = lockoutAttempts;

                    _companyInfo.RequireUppercase = RequireUppercaseCheckBox.IsChecked ?? false;
                    _companyInfo.RequireLowercase = RequireLowercaseCheckBox.IsChecked ?? false;
                    _companyInfo.RequireDigit = RequireDigitCheckBox.IsChecked ?? false;
                    _companyInfo.RequireSpecialChar = RequireSpecialCharCheckBox.IsChecked ?? false;
                    _companyInfo.DefaultRoleId = DefaultRoleComboBox.SelectedValue as int?;

                    db.CompanyInfos.Update(_companyInfo);
                    db.SaveChanges();
                    MessageBox.Show("تم حفظ إعدادات المستخدمين بنجاح", "نجاح");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ إعدادات المستخدمين: {ex.Message}", "خطأ");
            }
        }

        private void SaveNumberingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var sequences = NumberingDataGrid.ItemsSource as IEnumerable<NumberingSequence>;
                    if (sequences != null)
                    {
                        db.NumberingSequences.UpdateRange(sequences);
                        db.SaveChanges();
                        MessageBox.Show("تم حفظ إعدادات الترقيم بنجاح", "نجاح");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ إعدادات الترقيم: {ex.Message}", "خطأ");
            }
        }

        private void SaveNotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var notifications = NotificationsDataGrid.ItemsSource as IEnumerable<NotificationSetting>;
                    if (notifications != null)
                    {
                        db.NotificationSettings.UpdateRange(notifications);
                        db.SaveChanges();
                        MessageBox.Show("تم حفظ إعدادات الإشعارات بنجاح", "نجاح");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ إعدادات الإشعارات: {ex.Message}", "خطأ");
            }
        }

        private void SaveDefaultAccountsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    if (_companyInfo == null) return;

                    _companyInfo.DefaultSalesAccountId = SalesAccountComboBox.SelectedValue as int?;
                    _companyInfo.DefaultAccountsReceivableId = AccountsReceivableComboBox.SelectedValue as int?;
                    _companyInfo.DefaultPurchasesAccountId = PurchasesAccountComboBox.SelectedValue as int?;
                    _companyInfo.DefaultAccountsPayableId = AccountsPayableComboBox.SelectedValue as int?;

                    db.CompanyInfos.Update(_companyInfo);
                    db.SaveChanges();
                    MessageBox.Show("تم حفظ الحسابات الافتراضية بنجاح", "نجاح");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ الحسابات الافتراضية: {ex.Message}", "خطأ");
            }
        }

        private void UploadLogoButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "ملفات الصور|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "اختر شعار المصنع"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _logoBytes = File.ReadAllBytes(openFileDialog.FileName);
                    DisplayLogo();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل تحميل الشعار: {ex.Message}", "خطأ");
                }
            }
        }

        private void DisplayLogo()
        {
            if (_logoBytes != null && _logoBytes.Length > 0)
            {
                using (var stream = new MemoryStream(_logoBytes))
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                    LogoImage.Source = bitmapImage;
                }
            }
            else
            {
                LogoImage.Source = null;
            }
        }

        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var backupFileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                var backupPath = Path.Combine(_backupFolder, backupFileName);
                File.Copy(_dbPath, backupPath);
                LoadBackups();
                MessageBox.Show($"تم إنشاء نسخة احتياطية بنجاح في: {backupPath}", "نجاح");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل إنشاء نسخة احتياطية: {ex.Message}", "خطأ");
            }
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (BackupsListView.SelectedItem is BackupFileViewModel selectedBackup)
            {
                try
                {
                    File.Copy(selectedBackup.FilePath, _dbPath, overwrite: true);
                    MessageBox.Show("تم استعادة النسخة الاحتياطية بنجاح. يرجى إعادة تشغيل التطبيق لتطبيق التغييرات.", "نجاح");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل استعادة النسخة الاحتياطية: {ex.Message}", "خطأ");
                }
            }
            else
            {
                MessageBox.Show("الرجاء تحديد نسخة احتياطية لاستعادتها", "تحذير");
            }
        }

        private void DeleteBackupButton_Click(object sender, RoutedEventArgs e)
        {
            if (BackupsListView.SelectedItem is BackupFileViewModel selectedBackup)
            {
                try
                {
                    File.Delete(selectedBackup.FilePath);
                    LoadBackups();
                    MessageBox.Show("تم حذف النسخة الاحتياطية بنجاح", "نجاح");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل حذف النسخة الاحتياطية: {ex.Message}", "خطأ");
                }
            }
            else
            {
                MessageBox.Show("الرجاء تحديد نسخة احتياطية لحذفها", "تحذير");
            }
        }

        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var selectedTheme = (sender as RadioButton)?.Content.ToString();
            if (selectedTheme == "فاتح (Light)")
            {
                // تطبيق المظهر الفاتح
                Application.Current.Resources.MergedDictionaries[0].Source =
                    new Uri("Themes/LightTheme.xaml", UriKind.Relative);
            }
            else if (selectedTheme == "داكن (Dark)")
            {
                // تطبيق المظهر الداكن
                Application.Current.Resources.MergedDictionaries[0].Source =
                    new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
            }
        }
    }
}