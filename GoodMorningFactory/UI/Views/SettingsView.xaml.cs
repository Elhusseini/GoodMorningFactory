// UI/Views/SettingsView.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using GoodMorningFactory.Core.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace GoodMorningFactory.UI.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly string _appDataFolder;
        private readonly string _dbPath;
        private readonly string _backupFolder;
        private CompanyInfo _companyInfo;
        private byte[]? _logoBytes;

        public SettingsView()
        {
            InitializeComponent();
            _appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GoodMorningFactory");
            _dbPath = Path.Combine(_appDataFolder, "GoodMorningFactory.db");
            _backupFolder = Path.Combine(_appDataFolder, "Backups");

            Directory.CreateDirectory(_appDataFolder);
            Directory.CreateDirectory(_backupFolder);

            LoadCompanyInfo();
            LoadGeneralSettingsOptions();
            LoadUserRoleSettingsOptions();
            LoadDefaultAccounts();
            LoadInventorySettings(); // *** إضافة جديدة ***
            LoadBackupSettings(); // استدعاء الدالة الجديدة
            LoadNumberingSequences();
            LoadNotificationSettings();
            LoadBackups();
        }

        #region Load Methods
        private void LoadCompanyInfo()
        {
            using (var db = new DatabaseContext())
            {
                _companyInfo = db.CompanyInfos.FirstOrDefault() ?? new CompanyInfo();
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
                LanguageComboBox.SelectedItem = _companyInfo.DefaultLanguage ?? "العربية";
                DateFormatComboBox.SelectedItem = _companyInfo.DefaultDateFormat ?? "dd/MM/yyyy";
                MinPassLengthTextBox.Text = _companyInfo.MinPasswordLength.ToString();
                PassExpiryTextBox.Text = _companyInfo.PasswordExpiryDays.ToString();
                LockoutAttemptsTextBox.Text = _companyInfo.FailedLoginLockoutAttempts.ToString();
                RequireUppercaseCheckBox.IsChecked = _companyInfo.RequireUppercase;
                RequireLowercaseCheckBox.IsChecked = _companyInfo.RequireLowercase;
                RequireDigitCheckBox.IsChecked = _companyInfo.RequireDigit;
                RequireSpecialCharCheckBox.IsChecked = _companyInfo.RequireSpecialChar;
            }
        }

        private void LoadGeneralSettingsOptions()
        {
            try
            {
                LanguageComboBox.ItemsSource = new[] { "العربية", "الإنجليزية" };
                DateFormatComboBox.ItemsSource = new[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
                using (var db = new DatabaseContext())
                {
                    CurrencyComboBox.ItemsSource = db.Currencies.Where(c => c.IsActive).ToList();
                    if (_companyInfo?.DefaultCurrencyId != null)
                    {
                        CurrencyComboBox.SelectedValue = _companyInfo.DefaultCurrencyId;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل الإعدادات العامة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUserRoleSettingsOptions()
        {
            using (var db = new DatabaseContext())
            {
                DefaultRoleComboBox.ItemsSource = db.Roles.ToList();
                if (_companyInfo?.DefaultRoleId != null)
                {
                    DefaultRoleComboBox.SelectedValue = _companyInfo.DefaultRoleId;
                }
            }
        }

        private void LoadDefaultAccounts()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var accounts = db.Accounts.OrderBy(a => a.AccountNumber).ToList();
                    var expenseAccounts = accounts.Where(a => a.AccountType == AccountType.Expense).ToList();
                    var assetAccounts = accounts.Where(a => a.AccountType == AccountType.Asset).ToList();
                    var liabilityAccounts = accounts.Where(a => a.AccountType == AccountType.Liability).ToList();

                    SalesAccountComboBox.ItemsSource = new List<Account>(accounts);
                    AccountsReceivableComboBox.ItemsSource = new List<Account>(accounts);
                    PurchasesAccountComboBox.ItemsSource = new List<Account>(accounts);
                    AccountsPayableComboBox.ItemsSource = new List<Account>(accounts);
                    CashAccountComboBox.ItemsSource = assetAccounts;
                    InventoryAccountComboBox.ItemsSource = assetAccounts;
                    PurchaseReturnsAccountComboBox.ItemsSource = new List<Account>(accounts);
                    PayrollExpenseAccountComboBox.ItemsSource = expenseAccounts;
                    PayrollAccrualAccountComboBox.ItemsSource = liabilityAccounts;
                    CogsAccountComboBox.ItemsSource = expenseAccounts;
                    VatAccountComboBox.ItemsSource = liabilityAccounts;
                    InventoryAdjustmentAccountComboBox.ItemsSource = expenseAccounts;

                    if (_companyInfo != null)
                    {
                        SalesAccountComboBox.SelectedValue = _companyInfo.DefaultSalesAccountId;
                        AccountsReceivableComboBox.SelectedValue = _companyInfo.DefaultAccountsReceivableAccountId;
                        PurchasesAccountComboBox.SelectedValue = _companyInfo.DefaultPurchasesAccountId;
                        AccountsPayableComboBox.SelectedValue = _companyInfo.DefaultAccountsPayableAccountId;
                        CashAccountComboBox.SelectedValue = _companyInfo.DefaultCashAccountId;
                        InventoryAccountComboBox.SelectedValue = _companyInfo.DefaultInventoryAccountId;
                        PurchaseReturnsAccountComboBox.SelectedValue = _companyInfo.DefaultPurchaseReturnsAccountId;
                        PayrollExpenseAccountComboBox.SelectedValue = _companyInfo.DefaultPayrollExpenseAccountId;
                        PayrollAccrualAccountComboBox.SelectedValue = _companyInfo.DefaultPayrollAccrualAccountId;
                        CogsAccountComboBox.SelectedValue = _companyInfo.DefaultCogsAccountId;
                        VatAccountComboBox.SelectedValue = _companyInfo.DefaultVatAccountId;
                        InventoryAdjustmentAccountComboBox.SelectedValue = _companyInfo.DefaultInventoryAdjustmentAccountId;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل الحسابات الافتراضية: {ex.Message}"); }
        }

        // --- بداية الإضافة: دالة تحميل إعدادات المخزون ---
        private void LoadInventorySettings()
        {
            try
            {
                CostingMethodComboBox.ItemsSource = Enum.GetValues(typeof(InventoryCostingMethod))
                    .Cast<InventoryCostingMethod>()
                    .Select(e => new { Value = e, Description = GetEnumDescription(e) });

                if (_companyInfo != null)
                {
                    CostingMethodComboBox.SelectedValue = _companyInfo.DefaultCostingMethod;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل إعدادات المخزون: {ex.Message}");
            }
        }
        // --- نهاية الإضافة ---

        private void LoadNumberingSequences()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var settings = db.NumberingSequences.ToList();
                    var allDocTypes = Enum.GetValues(typeof(DocumentType)).Cast<DocumentType>();

                    var sequencesToDisplay = new List<NumberingSequence>();
                    foreach (var docType in allDocTypes)
                    {
                        var setting = settings.FirstOrDefault(s => s.DocumentType == docType);
                        if (setting == null)
                        {
                            sequencesToDisplay.Add(new NumberingSequence { DocumentType = docType, LastNumber = 0, NumberOfDigits = 4 });
                        }
                        else
                        {
                            sequencesToDisplay.Add(setting);
                        }
                    }
                    NumberingDataGrid.ItemsSource = sequencesToDisplay.OrderBy(s => s.DocumentType.ToString()).ToList();
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل إعدادات الترقيم: {ex.Message}"); }
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
            catch (Exception ex) { MessageBox.Show($"فشل تحميل إعدادات الإشعارات: {ex.Message}"); }
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
                    .OrderByDescending(f => f.CreationDate).ToList();
                BackupsListView.ItemsSource = backupFiles;
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل قائمة النسخ الاحتياطية: {ex.Message}"); }
        }

        // --- بداية الإضافة: دالة تحميل إعدادات النسخ الاحتياطي ---
        private void LoadBackupSettings()
        {
            if (_companyInfo != null)
            {
                AutoBackupCheckBox.IsChecked = _companyInfo.IsAutoBackupEnabled;
                BackupsToKeepTextBox.Text = _companyInfo.BackupsToKeep.ToString();
            }
        }
        // --- نهاية الإضافة ---

        #endregion

        #region Save Methods
        private CompanyInfo GetOrCreateCompanyInfo(DatabaseContext db)
        {
            var info = db.CompanyInfos.FirstOrDefault();
            if (info == null)
            {
                info = new CompanyInfo();
                db.CompanyInfos.Add(info);
            }
            return info;
        }

        private void SaveCompanyInfoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var info = GetOrCreateCompanyInfo(db);
                    info.CompanyName = CompanyNameTextBox.Text;
                    info.Address = AddressTextBox.Text;
                    info.City = CityTextBox.Text;
                    info.Country = CountryTextBox.Text;
                    info.PhoneNumber = PhoneNumberTextBox.Text;
                    info.Email = EmailTextBox.Text;
                    info.Website = WebsiteTextBox.Text;
                    info.TaxNumber = TaxNumberTextBox.Text;
                    info.CommercialRegistrationNumber = CommercialRegTextBox.Text;
                    info.Logo = _logoBytes;
                    db.SaveChanges();
                    MessageBox.Show("تم حفظ معلومات المصنع بنجاح.", "نجاح");
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل حفظ المعلومات: {ex.Message}"); }
        }

        private void SaveGeneralSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var info = GetOrCreateCompanyInfo(db);
                    var selectedCurrencyId = (int?)CurrencyComboBox.SelectedValue;
                    info.DefaultLanguage = LanguageComboBox.SelectedItem as string;
                    info.DefaultDateFormat = DateFormatComboBox.SelectedItem as string;
                    info.DefaultCurrencyId = selectedCurrencyId;
                    if (selectedCurrencyId.HasValue)
                    {
                        var allCurrencies = db.Currencies.ToList();
                        foreach (var currency in allCurrencies)
                        {
                            currency.IsDefault = (currency.Id == selectedCurrencyId.Value);
                        }
                    }
                    db.SaveChanges();
                    Core.Services.AppSettings.LoadSettings();
                    MessageBox.Show("تم حفظ الإعدادات العامة بنجاح.", "نجاح");
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل حفظ الإعدادات: {ex.Message}\n{ex.InnerException?.Message}", "خطأ"); }
        }

        private void SaveUserSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var info = GetOrCreateCompanyInfo(db);
                    int.TryParse(MinPassLengthTextBox.Text, out int minLength);
                    info.MinPasswordLength = minLength > 0 ? minLength : 8;
                    info.RequireUppercase = RequireUppercaseCheckBox.IsChecked ?? false;
                    info.RequireLowercase = RequireLowercaseCheckBox.IsChecked ?? false;
                    info.RequireDigit = RequireDigitCheckBox.IsChecked ?? false;
                    info.RequireSpecialChar = RequireSpecialCharCheckBox.IsChecked ?? false;
                    info.DefaultRoleId = (int?)DefaultRoleComboBox.SelectedValue;
                    db.SaveChanges();
                    MessageBox.Show("تم حفظ إعدادات المستخدمين بنجاح.", "نجاح");
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل حفظ الإعدادات: {ex.Message}"); }
        }

        private void SaveDefaultAccountsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var info = GetOrCreateCompanyInfo(db);
                    info.DefaultSalesAccountId = (int?)SalesAccountComboBox.SelectedValue;
                    info.DefaultAccountsReceivableAccountId = (int?)AccountsReceivableComboBox.SelectedValue;
                    info.DefaultPurchasesAccountId = (int?)PurchasesAccountComboBox.SelectedValue;
                    info.DefaultAccountsPayableAccountId = (int?)AccountsPayableComboBox.SelectedValue;
                    info.DefaultCashAccountId = (int?)CashAccountComboBox.SelectedValue;
                    info.DefaultInventoryAccountId = (int?)InventoryAccountComboBox.SelectedValue;
                    info.DefaultPurchaseReturnsAccountId = (int?)PurchaseReturnsAccountComboBox.SelectedValue;
                    info.DefaultPayrollExpenseAccountId = (int?)PayrollExpenseAccountComboBox.SelectedValue;
                    info.DefaultPayrollAccrualAccountId = (int?)PayrollAccrualAccountComboBox.SelectedValue;
                    info.DefaultCogsAccountId = (int?)CogsAccountComboBox.SelectedValue;
                    info.DefaultVatAccountId = (int?)VatAccountComboBox.SelectedValue;
                    info.DefaultInventoryAdjustmentAccountId = (int?)InventoryAdjustmentAccountComboBox.SelectedValue;
                    db.SaveChanges();
                    MessageBox.Show("تم حفظ الحسابات الافتراضية بنجاح.", "نجاح");
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل حفظ الإعدادات: {ex.Message}"); }
        }

        // --- بداية الإضافة: دالة حفظ إعدادات المخزون ---
        private void SaveInventorySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (CostingMethodComboBox.SelectedValue == null)
            {
                MessageBox.Show("يرجى اختيار طريقة تقييم المخزون.", "بيانات ناقصة");
                return;
            }
            try
            {
                using (var db = new DatabaseContext())
                {
                    var info = GetOrCreateCompanyInfo(db);
                    info.DefaultCostingMethod = (InventoryCostingMethod)CostingMethodComboBox.SelectedValue;
                    db.SaveChanges();
                    MessageBox.Show("تم حفظ إعدادات المخزون بنجاح.", "نجاح");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ إعدادات المخزون: {ex.Message}", "خطأ");
            }
        }
        // --- نهاية الإضافة ---

        private void SaveNumberingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var sequencesFromGrid = NumberingDataGrid.ItemsSource as List<NumberingSequence>;
                    if (sequencesFromGrid == null) return;

                    foreach (var seq in sequencesFromGrid)
                    {
                        var seqInDb = db.NumberingSequences.FirstOrDefault(s => s.DocumentType == seq.DocumentType);
                        if (seqInDb != null)
                        {
                            seqInDb.Prefix = seq.Prefix;
                            seqInDb.LastNumber = seq.LastNumber;
                            seqInDb.NumberOfDigits = seq.NumberOfDigits;
                        }
                        else
                        {
                            db.NumberingSequences.Add(new NumberingSequence
                            {
                                DocumentType = seq.DocumentType,
                                Prefix = seq.Prefix,
                                LastNumber = seq.LastNumber,
                                NumberOfDigits = seq.NumberOfDigits
                            });
                        }
                    }
                    db.SaveChanges();
                    MessageBox.Show("تم حفظ إعدادات الترقيم بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل حفظ إعدادات الترقيم: {ex.InnerException?.Message ?? ex.Message}", "خطأ"); }
        }

        private void SaveNotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var settings = (List<NotificationSetting>)NotificationsDataGrid.ItemsSource;
                    foreach (var setting in settings)
                    {
                        var existing = db.NotificationSettings.Find(setting.Id);
                        if (existing != null)
                        {
                            db.Entry(existing).CurrentValues.SetValues(setting);
                        }
                        else
                        {
                            db.NotificationSettings.Add(setting);
                        }
                    }
                    db.SaveChanges();
                    MessageBox.Show("تم حفظ إعدادات الإشعارات بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل حفظ إعدادات الإشعارات: {ex.Message}", "خطأ"); }
        }


        // --- بداية الإضافة: دالة حفظ إعدادات النسخ الاحتياطي ---
        private void SaveBackupSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(BackupsToKeepTextBox.Text, out int backupsToKeep) || backupsToKeep < 1)
            {
                MessageBox.Show("يرجى إدخال عدد صحيح وصالح للنسخ التي سيتم الاحتفاظ بها (1 على الأقل).", "خطأ في الإدخال");
                return;
            }

            try
            {
                using (var db = new DatabaseContext())
                {
                    var info = GetOrCreateCompanyInfo(db);
                    info.IsAutoBackupEnabled = AutoBackupCheckBox.IsChecked ?? true;
                    info.BackupsToKeep = backupsToKeep;
                    db.SaveChanges();
                    MessageBox.Show("تم حفظ إعدادات النسخ الاحتياطي بنجاح.", "نجاح");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ الإعدادات: {ex.Message}", "خطأ");
            }
        }
        // --- نهاية الإضافة ---


        #endregion

        #region Other UI Event Handlers
        private void UploadLogoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "ملفات الصور (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg", Title = "اختر شعار الشركة" };
            if (openFileDialog.ShowDialog() == true)
            {
                _logoBytes = File.ReadAllBytes(openFileDialog.FileName);
                DisplayLogo();
            }
        }

        private void DisplayLogo()
        {
            if (_logoBytes != null && _logoBytes.Length > 0)
            {
                BitmapImage image = new BitmapImage();
                using (MemoryStream stream = new MemoryStream(_logoBytes))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                }
                LogoImage.Source = image;
            }
        }

        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string backupFileName = $"backup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.db";
                string backupFilePath = Path.Combine(_backupFolder, backupFileName);
                File.Copy(_dbPath, backupFilePath, true);
                MessageBox.Show($"تم إنشاء نسخة احتياطية بنجاح.\nالمسار: {backupFilePath}", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadBackups();
            }
            catch (Exception ex) { MessageBox.Show($"فشلت عملية النسخ الاحتياطي: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is BackupFileViewModel backup)
            {
                var result = MessageBox.Show($"هل أنت متأكد من استعادة النسخة '{backup.FileName}'؟\nسيتم الكتابة فوق قاعدة البيانات الحالية، وهذه العملية لا يمكن التراجع عنها. يوصى بإغلاق البرنامج وإعادة فتحه بعد الاستعادة.", "تحذير خطير!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        File.Copy(backup.FilePath, _dbPath, true);
                        MessageBox.Show("تمت استعادة النسخة الاحتياطية بنجاح. يرجى إعادة تشغيل البرنامج لتطبيق التغييرات.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex) { MessageBox.Show($"فشلت عملية الاستعادة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
            }
        }

        private void DeleteBackupButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is BackupFileViewModel backup)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف النسخة الاحتياطية '{backup.FileName}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        File.Delete(backup.FilePath);
                        MessageBox.Show("تم حذف ملف النسخة الاحتياطية.", "نجاح");
                        LoadBackups();
                    }
                    catch (Exception ex) { MessageBox.Show($"فشل حذف الملف: {ex.Message}", "خطأ"); }
                }
            }
        }

        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var selectedTheme = (sender as RadioButton)?.Content.ToString();
            try
            {
                if (selectedTheme == "فاتح (Light)")
                {
                    Application.Current.Resources.MergedDictionaries[0].Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
                }
                else if (selectedTheme == "داكن (Dark)")
                {
                    Application.Current.Resources.MergedDictionaries[0].Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل المظهر: {ex.Message}. تأكد من وجود ملفات الثيم في مجلد Themes.", "خطأ"); }
        }

        private void ManageCurrenciesButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.NavigateTo("Currencies");
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
}
