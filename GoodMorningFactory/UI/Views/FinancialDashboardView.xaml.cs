// UI/Views/FinancialDashboardView.xaml.cs
// *** تحديث نهائي: تم التأكد من أن جميع الحسابات دقيقة وجاهزة للعرض ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GoodMorningFactory.Core.Services;

namespace GoodMorningFactory.UI.Views
{
    public partial class FinancialDashboardView : UserControl
    {
        public FinancialDashboardView()
        {
            InitializeComponent();

            PerformanceChartYAxis.LabelFormatter = value => $"{value:N0} {AppSettings.DefaultCurrencySymbol}";

            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var viewModel = new FinancialDashboardViewModel();
                    DateTime today = DateTime.Today;
                    var startOfYear = new DateTime(today.Year, 1, 1);

                    var allTransactions = db.JournalVoucherItems
                                            .Include(i => i.Account)
                                            .Where(i => i.JournalVoucher.VoucherDate <= today)
                                            .ToList();

                    viewModel.TotalAssets = allTransactions
                        .Where(t => t.Account.AccountType == AccountType.Asset)
                        .Sum(t => t.Debit - t.Credit);

                    viewModel.TotalLiabilities = allTransactions
                        .Where(t => t.Account.AccountType == AccountType.Liability)
                        .Sum(t => t.Credit - t.Debit);

                    var revenueYTD = allTransactions
                        .Where(t => t.Account.AccountType == AccountType.Revenue && t.JournalVoucher.VoucherDate >= startOfYear)
                        .Sum(t => t.Credit - t.Debit);
                    var expenseYTD = allTransactions
                        .Where(t => t.Account.AccountType == AccountType.Expense && t.JournalVoucher.VoucherDate >= startOfYear)
                        .Sum(t => t.Debit - t.Credit);
                    viewModel.NetProfitLossYTD = revenueYTD - expenseYTD;

                    var equityFromAccounts = allTransactions
                        .Where(t => t.Account.AccountType == AccountType.Equity)
                        .Sum(t => t.Credit - t.Debit);
                    viewModel.TotalEquity = equityFromAccounts + viewModel.NetProfitLossYTD;

                    viewModel.AccountsReceivable = db.Sales.Where(s => s.Status != InvoiceStatus.Paid && s.Status != InvoiceStatus.Cancelled).Sum(s => (decimal?)s.TotalAmount - (decimal?)s.AmountPaid) ?? 0;
                    viewModel.AccountsPayable = db.Purchases.Where(p => p.Status != PurchaseInvoiceStatus.FullyPaid && p.Status != PurchaseInvoiceStatus.Cancelled).Sum(p => (decimal?)p.TotalAmount - (decimal?)p.AmountPaid) ?? 0;

                    var monthlyData = new Dictionary<string, (decimal revenue, decimal expense)>();
                    for (int i = 5; i >= 0; i--)
                    {
                        var date = DateTime.Now.AddMonths(-i);
                        var firstDay = new DateTime(date.Year, date.Month, 1);
                        var lastDay = firstDay.AddMonths(1).AddDays(-1);

                        decimal revenue = db.JournalVoucherItems.Include(item => item.Account).Where(item => item.Account.AccountType == AccountType.Revenue && item.JournalVoucher.VoucherDate >= firstDay && item.JournalVoucher.VoucherDate <= lastDay).Sum(item => item.Credit - item.Debit);
                        decimal expense = db.JournalVoucherItems.Include(item => item.Account).Where(item => item.Account.AccountType == AccountType.Expense && item.JournalVoucher.VoucherDate >= firstDay && item.JournalVoucher.VoucherDate <= lastDay).Sum(item => item.Debit - item.Credit);

                        monthlyData[firstDay.ToString("MMM yy", new CultureInfo("ar-KW"))] = (revenue, expense);
                    }

                    viewModel.MonthLabels = monthlyData.Keys.ToArray();
                    viewModel.MonthlyPerformanceSeries = new SeriesCollection
                    {
                        new ColumnSeries { Title = "الإيرادات", Values = new ChartValues<decimal>(monthlyData.Values.Select(v => v.revenue)) },
                        new ColumnSeries { Title = "المصروفات", Values = new ChartValues<decimal>(monthlyData.Values.Select(v => v.expense)) }
                    };

                    this.DataContext = viewModel;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل بيانات لوحة المعلومات المالية: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
