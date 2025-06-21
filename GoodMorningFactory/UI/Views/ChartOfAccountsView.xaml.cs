// UI/Views/ChartOfAccountsView.xaml.cs
// *** تحديث: تم استكمال منطق عرض الأرصدة والحذف الآمن ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class ChartOfAccountsView : UserControl
    {
        public ChartOfAccountsView()
        {
            InitializeComponent();
            LoadAccounts();
        }

        private void LoadAccounts()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var accounts = db.Accounts.ToList();
                    var transactions = db.JournalVoucherItems.ToList();
                    var accountViewModels = new List<AccountViewModel>();

                    // --- بداية التحديث: حساب الرصيد لكل حساب ---
                    foreach (var account in accounts)
                    {
                        decimal balance = transactions.Where(t => t.AccountId == account.Id).Sum(t => t.Debit - t.Credit);
                        accountViewModels.Add(new AccountViewModel(account) { Balance = balance });
                    }
                    // --- نهاية التحديث ---

                    var dictionary = accountViewModels.ToDictionary(vm => vm.Account.Id);
                    var rootAccounts = new ObservableCollection<AccountViewModel>();

                    foreach (var vm in accountViewModels)
                    {
                        if (vm.Account.ParentAccountId.HasValue && dictionary.ContainsKey(vm.Account.ParentAccountId.Value))
                        {
                            dictionary[vm.Account.ParentAccountId.Value].Children.Add(vm);
                        }
                        else
                        {
                            rootAccounts.Add(vm);
                        }
                    }
                    AccountsTreeView.ItemsSource = rootAccounts;
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل دليل الحسابات: {ex.Message}"); }
        }

        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditAccountWindow();
            if (addWindow.ShowDialog() == true) { LoadAccounts(); }
        }

        private void EditAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsTreeView.SelectedItem is AccountViewModel selectedAccount)
            {
                var editWindow = new AddEditAccountWindow(selectedAccount.Account.Id);
                if (editWindow.ShowDialog() == true) { LoadAccounts(); }
            }
            else { MessageBox.Show("يرجى تحديد حساب لتعديله."); }
        }

        // --- بداية التحديث: تفعيل منطق الحذف الآمن ---
        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsTreeView.SelectedItem is AccountViewModel selectedVm)
            {
                var accountToDelete = selectedVm.Account;

                // التحقق من وجود حسابات فرعية
                if (selectedVm.Children.Any())
                {
                    MessageBox.Show("لا يمكن حذف هذا الحساب لأنه يحتوي على حسابات فرعية. يرجى حذف الحسابات الفرعية أولاً.", "عملية غير ممكنة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // التحقق من وجود حركات مالية
                using (var db = new DatabaseContext())
                {
                    bool hasTransactions = db.JournalVoucherItems.Any(t => t.AccountId == accountToDelete.Id);
                    if (hasTransactions)
                    {
                        MessageBox.Show("لا يمكن حذف هذا الحساب لوجود حركات مالية مرتبطة به. يمكنك جعله 'غير نشط' بدلاً من ذلك.", "عملية غير ممكنة", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                var result = MessageBox.Show($"هل أنت متأكد من حذف الحساب '{accountToDelete.AccountName}' بشكل نهائي؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DatabaseContext())
                        {
                            db.Accounts.Remove(accountToDelete);
                            db.SaveChanges();
                            LoadAccounts();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"فشل حذف الحساب: {ex.Message}", "خطأ");
                    }
                }
            }
            else { MessageBox.Show("يرجى تحديد حساب لحذفه."); }
        }
        // --- نهاية التحديث ---
    }
}