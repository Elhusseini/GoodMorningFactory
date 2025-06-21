// UI/Views/AddEditAccountWindow.xaml.cs
// الكود الخلفي لنافذة إضافة وتعديل حساب
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditAccountWindow : Window
    {
        private readonly int? _accountId;
        private Account _accountToEdit;

        public AddEditAccountWindow(int? accountId = null)
        {
            InitializeComponent();
            _accountId = accountId;
            LoadData();
        }

        private void LoadData()
        {
            // ملء قائمة أنواع الحسابات
            AccountTypeComboBox.ItemsSource = Enum.GetValues(typeof(AccountType));
            AccountTypeComboBox.SelectedIndex = 0;

            using (var db = new DatabaseContext())
            {
                // ملء قائمة الحسابات الرئيسية المحتملة
                ParentAccountComboBox.ItemsSource = db.Accounts.ToList();

                // إذا كنا في وضع التعديل، نقوم بتحميل بيانات الحساب
                if (_accountId.HasValue)
                {
                    _accountToEdit = db.Accounts.Find(_accountId.Value);
                    if (_accountToEdit != null)
                    {
                        AccountNumberTextBox.Text = _accountToEdit.AccountNumber;
                        AccountNameTextBox.Text = _accountToEdit.AccountName;
                        AccountTypeComboBox.SelectedItem = _accountToEdit.AccountType;
                        ParentAccountComboBox.SelectedValue = _accountToEdit.ParentAccountId;
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AccountNumberTextBox.Text) ||
                string.IsNullOrWhiteSpace(AccountNameTextBox.Text) ||
                AccountTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("رقم الحساب، اسم الحساب، ونوع الحساب حقول مطلوبة.",
                              "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new DatabaseContext())
                {
                    if (_accountToEdit == null)
                    {
                        // إضافة حساب جديد
                        var newAccount = new Account
                        {
                            AccountNumber = AccountNumberTextBox.Text,
                            AccountName = AccountNameTextBox.Text,
                            AccountType = (AccountType)AccountTypeComboBox.SelectedItem,
                            ParentAccountId = (int?)ParentAccountComboBox.SelectedValue
                        };
                        db.Accounts.Add(newAccount);
                    }
                    else
                    {
                        // تعديل حساب موجود
                        var account = db.Accounts.Find(_accountId.Value);
                        if (account != null)
                        {
                            account.AccountNumber = AccountNumberTextBox.Text;
                            account.AccountName = AccountNameTextBox.Text;
                            account.AccountType = (AccountType)AccountTypeComboBox.SelectedItem;
                            account.ParentAccountId = (int?)ParentAccountComboBox.SelectedValue;
                        }
                    }

                    db.SaveChanges();
                    MessageBox.Show("تم حفظ الحساب بنجاح.", "نجاح",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية حفظ الحساب: {ex.InnerException?.Message ?? ex.Message}",
                              "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}