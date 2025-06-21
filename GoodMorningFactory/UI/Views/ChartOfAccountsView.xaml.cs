// UI/Views/ChartOfAccountsView.xaml.cs
// *** تحديث: تم إصلاح الخلل الأمني المتعلق بالصلاحيات ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GoodMorningFactory.Core.Services; // <-- إضافة مهمة

namespace GoodMorningFactory.UI.Views
{
    public partial class ChartOfAccountsView : UserControl
    {
        public ChartOfAccountsView()
        {
            InitializeComponent();
            ApplyPermissions(); // <-- بداية الإصلاح: استدعاء دالة تطبيق الصلاحيات
            LoadData();
        }

        // --- بداية الإصلاح: إضافة دالة لتطبيق الصلاحيات ---
        private void ApplyPermissions()
        {
            // التحقق مما إذا كان المستخدم يمتلك صلاحية "إدارة شجرة الحسابات"
            bool canManage = PermissionsService.CanAccess("Financials.ChartOfAccounts.Manage");

            // تفعيل أو تعطيل الأزرار بناءً على نتيجة التحقق
            AddButton.IsEnabled = canManage;
            EditButton.IsEnabled = canManage;
            DeleteButton.IsEnabled = canManage;
        }
        // --- نهاية الإصلاح ---

        public void LoadData()
        {
            using (var db = new DatabaseContext())
            {
                var allAccounts = db.Accounts.ToList();
                var accountLookup = allAccounts.ToDictionary(a => a.Id, a => new AccountViewModel(a));
                var rootNodes = new ObservableCollection<AccountViewModel>();

                foreach (var account in allAccounts)
                {
                    if (account.ParentAccountId.HasValue && accountLookup.ContainsKey(account.ParentAccountId.Value))
                    {
                        accountLookup[account.ParentAccountId.Value].Children.Add(accountLookup[account.Id]);
                    }
                    else
                    {
                        rootNodes.Add(accountLookup[account.Id]);
                    }
                }
                AccountsTreeView.ItemsSource = rootNodes;
            }
        }

        private void LoadLedgerForAccount(int accountId)
        {
            var selectedVm = AccountsTreeView.SelectedItem as AccountViewModel;
            LedgerHeader.Text = $"دفتر الأستاذ للحساب: {selectedVm?.DisplayName ?? ""}";

            using (var db = new DatabaseContext())
            {
                var ledgerEntries = db.JournalVoucherItems
                    .Include(i => i.JournalVoucher)
                    .Where(i => i.AccountId == accountId)
                    .OrderBy(i => i.JournalVoucher.VoucherDate)
                    .ToList();

                var ledgerViewModels = new List<LedgerEntryViewModel>();
                decimal currentBalance = 0;

                foreach (var entry in ledgerEntries)
                {
                    currentBalance += entry.Debit - entry.Credit;
                    ledgerViewModels.Add(new LedgerEntryViewModel
                    {
                        Date = entry.JournalVoucher.VoucherDate,
                        Reference = entry.JournalVoucher.VoucherNumber,
                        Description = entry.JournalVoucher.Description,
                        Debit = entry.Debit,
                        Credit = entry.Credit,
                        Balance = currentBalance
                    });
                }
                LedgerDataGrid.ItemsSource = ledgerViewModels;
            }
        }

        private void AccountsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is AccountViewModel selectedVM)
            {
                LoadLedgerForAccount(selectedVM.Id);
            }
            else
            {
                LedgerDataGrid.ItemsSource = null;
                LedgerHeader.Text = "دفتر الأستاذ العام";
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditAccountWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsTreeView.SelectedItem is AccountViewModel selectedAccount)
            {
                var editWindow = new AddEditAccountWindow(selectedAccount.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("يرجى تحديد حساب لتعديله.", "تنبيه");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsTreeView.SelectedItem is AccountViewModel selectedAccount)
            {
                if (selectedAccount.Children.Any())
                {
                    MessageBox.Show("لا يمكن حذف هذا الحساب لأنه يحتوي على حسابات فرعية.", "خطأ");
                    return;
                }

                var result = MessageBox.Show($"هل أنت متأكد من حذف الحساب: {selectedAccount.DisplayName}؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var db = new DatabaseContext())
                    {
                        if (db.JournalVoucherItems.Any(i => i.AccountId == selectedAccount.Id))
                        {
                            MessageBox.Show("لا يمكن حذف هذا الحساب لوجود حركات مرتبطة به. يمكنك جعله 'غير نشط'.", "خطأ");
                            return;
                        }

                        var accountToDelete = db.Accounts.Find(selectedAccount.Id);
                        if (accountToDelete != null)
                        {
                            db.Accounts.Remove(accountToDelete);
                            db.SaveChanges();
                            LoadData();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("يرجى تحديد حساب لحذفه.", "تنبيه");
            }
        }
    }
}
