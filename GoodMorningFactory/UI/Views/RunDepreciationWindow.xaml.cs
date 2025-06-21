// UI/Views/RunDepreciationWindow.xaml.cs
// *** تحديث: تم إصلاح الخطأ المتعلق بعدم وجود علاقة الربط ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public class DepreciationVoucherLine
    {
        public string AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    public partial class RunDepreciationWindow : Window
    {
        private List<DepreciationVoucherLine> _proposedVoucherLines = new List<DepreciationVoucherLine>();
        private decimal _totalDepreciation = 0;
        private int _expenseAccountId;
        private int _accumulatedAccountId;

        public RunDepreciationWindow()
        {
            InitializeComponent();
            MonthComboBox.ItemsSource = Enumerable.Range(1, 12);
            YearComboBox.ItemsSource = Enumerable.Range(DateTime.Now.Year - 5, 10);
            MonthComboBox.SelectedItem = DateTime.Now.Month;
            YearComboBox.SelectedItem = DateTime.Now.Year;
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            int year = (int)YearComboBox.SelectedItem;
            int month = (int)MonthComboBox.SelectedItem;
            var periodEndDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            _proposedVoucherLines.Clear();
            _totalDepreciation = 0;

            try
            {
                using (var db = new DatabaseContext())
                {
                    string expectedDescription = $"إهلاك الأصول الثابتة عن شهر {month}/{year}";
                    bool alreadyPosted = db.JournalVouchers.Any(jv => jv.Description == expectedDescription);
                    if (alreadyPosted)
                    {
                        MessageBox.Show("تم احتساب وترحيل قيد الإهلاك لهذه الفترة من قبل.", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
                        PostVoucherButton.IsEnabled = false;
                        return;
                    }

                    // *** بداية الإصلاح: استخدام Include بشكل صحيح ***
                    var assets = db.FixedAssets
                        .Include(a => a.DepreciationExpenseAccount)
                        .Include(a => a.AccumulatedDepreciationAccount)
                        .Where(a => !a.IsDisposed && a.AcquisitionDate <= periodEndDate)
                        .ToList();
                    // *** نهاية الإصلاح ***

                    if (!assets.Any())
                    {
                        MessageBox.Show("لا توجد أصول لاحتساب إهلاك لها في هذه الفترة.", "معلومة");
                        PostVoucherButton.IsEnabled = false;
                        return;
                    }

                    foreach (var asset in assets)
                    {
                        if (asset.UsefulLifeYears == 0) continue;

                        decimal yearlyDepreciation = (asset.AcquisitionCost - asset.SalvageValue) / asset.UsefulLifeYears;
                        decimal monthlyDepreciation = yearlyDepreciation / 12;

                        decimal previousAccumulated = db.JournalVoucherItems
                            .Where(i => i.AccountId == asset.AccumulatedDepreciationAccountId)
                            .Sum(i => (decimal?)i.Credit) ?? 0;

                        if ((previousAccumulated + monthlyDepreciation) <= (asset.AcquisitionCost - asset.SalvageValue))
                        {
                            _totalDepreciation += monthlyDepreciation;
                        }
                    }

                    if (_totalDepreciation > 0)
                    {
                        // نفترض أن جميع الأصول تستخدم نفس حسابات المصروف والمجمع
                        _expenseAccountId = assets.First().DepreciationExpenseAccountId;
                        _accumulatedAccountId = assets.First().AccumulatedDepreciationAccountId;

                        _proposedVoucherLines.Add(new DepreciationVoucherLine { AccountName = assets.First().DepreciationExpenseAccount.AccountName, Debit = _totalDepreciation, Credit = 0 });
                        _proposedVoucherLines.Add(new DepreciationVoucherLine { AccountName = assets.First().AccumulatedDepreciationAccount.AccountName, Debit = 0, Credit = _totalDepreciation });

                        VoucherDescriptionTextBlock.Text = expectedDescription;
                        VoucherDetailsGrid.ItemsSource = null;
                        VoucherDetailsGrid.ItemsSource = _proposedVoucherLines;
                        PostVoucherButton.IsEnabled = true;
                    }
                    else
                    {
                        MessageBox.Show("لا توجد قيمة إهلاك ليتم احتسابها لهذه الفترة.", "معلومة");
                        PostVoucherButton.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل احتساب الإهلاك: {ex.Message}", "خطأ");
            }
        }

        private void PostVoucherButton_Click(object sender, RoutedEventArgs e)
        {
            int year = (int)YearComboBox.SelectedItem;
            int month = (int)MonthComboBox.SelectedItem;
            string description = $"إهلاك الأصول الثابتة عن شهر {month}/{year}";

            try
            {
                using (var db = new DatabaseContext())
                {
                    var voucher = new JournalVoucher
                    {
                        VoucherNumber = $"DEP-{year}-{month}",
                        VoucherDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)),
                        Description = description,
                        TotalDebit = _totalDepreciation,
                        TotalCredit = _totalDepreciation,
                        Status = VoucherStatus.Posted
                    };

                    voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = _expenseAccountId, Debit = _totalDepreciation, Credit = 0 });
                    voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = _accumulatedAccountId, Debit = 0, Credit = _totalDepreciation });

                    db.JournalVouchers.Add(voucher);
                    db.SaveChanges();

                    MessageBox.Show("تم ترحيل قيد الإهلاك بنجاح.", "نجاح");
                    PostVoucherButton.IsEnabled = false;
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل ترحيل القيد: {ex.Message}", "خطأ");
            }
        }
    }
}
