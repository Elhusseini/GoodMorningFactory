// UI/Views/FinancialDashboardView.xaml.cs
// *** ملف جديد: الكود الخلفي للوحة المعلومات المالية ***
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
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class FinancialDashboardView : UserControl
    {
        public FinancialDashboardView()
        {
            InitializeComponent();
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

                    viewModel.AccountsReceivable = db.Sales.Where(s => s.Status != InvoiceStatus.Paid).Sum(s => s.TotalAmount - s.AmountPaid);
                    viewModel.AccountsPayable = db.Purchases.Where(p => p.Status != PurchaseInvoiceStatus.FullyPaid).Sum(p => p.TotalAmount - p.AmountPaid);

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

                    viewModel.NetProfitLossYTD = db.JournalVoucherItems.Include(item => item.Account).Where(item => item.Account.AccountType == AccountType.Revenue && item.JournalVoucher.VoucherDate >= startOfYear).Sum(item => item.Credit - item.Debit) -
                                                 db.JournalVoucherItems.Include(item => item.Account).Where(item => item.Account.AccountType == AccountType.Expense && item.JournalVoucher.VoucherDate >= startOfYear).Sum(item => item.Debit - item.Credit);

                    this.DataContext = viewModel;
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }
    }
}