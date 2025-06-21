using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class TrialBalanceView : UserControl
    {
        public TrialBalanceView()
        {
            InitializeComponent();
            ToDatePicker.SelectedDate = System.DateTime.Today;
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (ToDatePicker.SelectedDate == null)
            {
                MessageBox.Show("يرجى تحديد تاريخ نهاية الفترة.", "تنبيه");
                return;
            }

            var toDate = ToDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1); // End of the selected day

            try
            {
                using (var db = new DatabaseContext())
                {
                    // Efficiently get all balances up to the selected date
                    var accountBalances = db.JournalVoucherItems
                        .Where(i => i.JournalVoucher.VoucherDate <= toDate)
                        .GroupBy(i => i.Account)
                        .Select(g => new
                        {
                            Account = g.Key,
                            TotalDebit = g.Sum(i => i.Debit),
                            TotalCredit = g.Sum(i => i.Credit)
                        })
                        .ToList();

                    var trialBalanceItems = new List<TrialBalanceItemViewModel>();
                    decimal totalDebit = 0;
                    decimal totalCredit = 0;

                    foreach (var item in accountBalances)
                    {
                        var balance = item.TotalDebit - item.TotalCredit;
                        if (balance == 0) continue; // Skip accounts with zero balance

                        var vm = new TrialBalanceItemViewModel
                        {
                            AccountNumber = item.Account.AccountNumber,
                            AccountName = item.Account.AccountName
                        };

                        if (balance > 0)
                        {
                            vm.DebitBalance = balance;
                            totalDebit += balance;
                        }
                        else
                        {
                            vm.CreditBalance = -balance; // Show as a positive number
                            totalCredit += -balance;
                        }
                        trialBalanceItems.Add(vm);
                    }

                    TrialBalanceDataGrid.ItemsSource = trialBalanceItems.OrderBy(i => i.AccountNumber);
                    TotalDebitTextBlock.Text = totalDebit.ToString("N2");
                    TotalCreditTextBlock.Text = totalCredit.ToString("N2");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء إنشاء التقرير: {ex.Message}", "خطأ");
            }
        }
    }
}
