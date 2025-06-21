using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// خدمة مركزية مسؤولة عن جلب وتجميع بيانات لوحة المعلومات المالية.
    /// </summary>
    public class FinancialDashboardService : IFinancialDashboardService
    {
        public async Task<FinancialKpisDto> GetFinancialKpisAsync()
        {
            using (var db = new DatabaseContext())
            {
                var today = DateTime.Today;
                var startOfYear = new DateTime(today.Year, 1, 1);

                // جلب جميع الحركات المالية حتى تاريخ اليوم مرة واحدة لتحسين الأداء
                var allTransactions = await db.JournalVoucherItems
                    .Include(i => i.Account)
                    .Where(i => i.JournalVoucher.VoucherDate <= today)
                    .ToListAsync();

                var kpis = new FinancialKpisDto();

                // حساب الأصول والخصوم وحقوق الملكية من دفتر اليومية
                kpis.TotalAssets = allTransactions
                    .Where(t => t.Account.AccountType == AccountType.Asset)
                    .Sum(t => t.Debit - t.Credit);

                kpis.TotalLiabilities = allTransactions
                    .Where(t => t.Account.AccountType == AccountType.Liability)
                    .Sum(t => t.Credit - t.Debit);

                decimal equityFromAccounts = allTransactions
                    .Where(t => t.Account.AccountType == AccountType.Equity)
                    .Sum(t => t.Credit - t.Debit);

                // حساب صافي الربح/الخسارة للسنة الحالية
                var revenueYTD = allTransactions
                    .Where(t => t.Account.AccountType == AccountType.Revenue && t.JournalVoucher.VoucherDate >= startOfYear)
                    .Sum(t => t.Credit - t.Debit);

                var expenseYTD = allTransactions
                    .Where(t => t.Account.AccountType == AccountType.Expense && t.JournalVoucher.VoucherDate >= startOfYear)
                    .Sum(t => t.Debit - t.Credit);

                kpis.NetProfitLossYTD = revenueYTD - expenseYTD;
                kpis.TotalEquity = equityFromAccounts + kpis.NetProfitLossYTD;

                // حساب الذمم المدينة والدائنة من فواتير المبيعات والمشتريات
                var arTask = db.Sales.Where(s => s.Status != InvoiceStatus.Paid && s.Status != InvoiceStatus.Cancelled).SumAsync(s => (decimal?)s.TotalAmount - (decimal?)s.AmountPaid);
                var apTask = db.Purchases.Where(p => p.Status != PurchaseInvoiceStatus.FullyPaid && p.Status != PurchaseInvoiceStatus.Cancelled).SumAsync(p => (decimal?)p.TotalAmount - (decimal?)p.AmountPaid);

                await Task.WhenAll(arTask, apTask);

                kpis.AccountsReceivable = await arTask ?? 0;
                kpis.AccountsPayable = await apTask ?? 0;

                return kpis;
            }
        }

        public async Task<Dictionary<string, (decimal revenue, decimal expense)>> GetMonthlyPerformanceAsync()
        {
            using (var db = new DatabaseContext())
            {
                var monthlyData = new Dictionary<string, (decimal revenue, decimal expense)>();
                for (int i = 5; i >= 0; i--)
                {
                    var date = DateTime.Now.AddMonths(-i);
                    var firstDay = new DateTime(date.Year, date.Month, 1);
                    var lastDay = firstDay.AddMonths(1).AddDays(-1);

                    var revenueTask = db.JournalVoucherItems.Include(item => item.Account).Where(item => item.Account.AccountType == AccountType.Revenue && item.JournalVoucher.VoucherDate >= firstDay && item.JournalVoucher.VoucherDate <= lastDay).SumAsync(item => item.Credit - item.Debit);
                    var expenseTask = db.JournalVoucherItems.Include(item => item.Account).Where(item => item.Account.AccountType == AccountType.Expense && item.JournalVoucher.VoucherDate >= firstDay && item.JournalVoucher.VoucherDate <= lastDay).SumAsync(item => item.Debit - item.Credit);

                    await Task.WhenAll(revenueTask, expenseTask);

                    string monthLabel = firstDay.ToString("MMM yy", new CultureInfo("ar-KW"));
                    monthlyData[monthLabel] = (await revenueTask, await expenseTask);
                }
                return monthlyData;
            }
        }
    }
}
