// UI/Views/BankReconciliationView.xaml.cs
// *** الكود الكامل للكود الخلفي لواجهة التسوية البنكية ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GoodMorningFactory.UI.Views
{
    public partial class BankReconciliationView : UserControl
    {
        private ObservableCollection<BankReconciliationViewModel> _transactions = new ObservableCollection<BankReconciliationViewModel>();
        private decimal _bookBalance = 0;

        public BankReconciliationView()
        {
            InitializeComponent();
            TransactionsDataGrid.ItemsSource = _transactions;
            LoadBankAccounts();
            _transactions.CollectionChanged += (s, e) => { if (e.NewItems != null) foreach (BankReconciliationViewModel item in e.NewItems) item.PropertyChanged += (s, e) => UpdateSummary(); };
        }

        private void LoadBankAccounts()
        {
            using (var db = new DatabaseContext())
            {
                BankAccountComboBox.ItemsSource = db.Accounts.Where(a => a.AccountName.Contains("بنك")).ToList();
            }
        }

        private void BankAccountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadUnreconciledTransactions();
        }

        private void LoadUnreconciledTransactions()
        {
            if (BankAccountComboBox.SelectedItem == null) return;
            int accountId = (int)BankAccountComboBox.SelectedValue;

            _transactions.Clear();

            using (var db = new DatabaseContext())
            {
                _bookBalance = db.JournalVoucherItems.Where(i => i.AccountId == accountId).Sum(i => i.Debit - i.Credit);

                var unreconciled = db.JournalVoucherItems
                    .Where(i => i.AccountId == accountId && !i.IsReconciled)
                    .Select(i => new BankReconciliationViewModel
                    {
                        JournalItemId = i.Id,
                        Date = i.JournalVoucher.VoucherDate,
                        Description = i.Description ?? i.JournalVoucher.Description,
                        Debit = i.Debit,
                        Credit = i.Credit,
                        IsSelected = false
                    })
                    .ToList();

                unreconciled.ForEach(t => _transactions.Add(t));
            }
            UpdateSummary();
        }

        private void StatementDateOrBalanceChanged(object sender, RoutedEventArgs e) => UpdateSummary();
        private void StatementDateOrBalanceChanged(object sender, TextChangedEventArgs e) => UpdateSummary();

        private void UpdateSummary()
        {
            decimal clearedDebits = _transactions.Where(t => t.IsSelected).Sum(t => t.Debit);
            decimal clearedCredits = _transactions.Where(t => t.IsSelected).Sum(t => t.Credit);
            decimal adjustedBalance = _bookBalance - clearedDebits + clearedCredits;

            BookBalanceTextBlock.Text = _bookBalance.ToString("C");
            ClearedDebitsTextBlock.Text = clearedDebits.ToString("C");
            ClearedCreditsTextBlock.Text = clearedCredits.ToString("C");
            AdjustedBalanceTextBlock.Text = adjustedBalance.ToString("C");

            decimal.TryParse(StatementBalanceTextBox.Text, out decimal statementBalance);
            decimal difference = adjustedBalance - statementBalance;
            DifferenceTextBlock.Text = difference.ToString("C");
            DifferenceTextBlock.Foreground = difference == 0 ? Brushes.Green : Brushes.Red;
        }

        private void SaveReconciliationButton_Click(object sender, RoutedEventArgs e)
        {
            // ... (منطق الحفظ يمكن إضافته هنا)
            MessageBox.Show("سيتم هنا حفظ التسوية وتحديث حالة الحركات.", "تحت الإنشاء");
        }
    }
}