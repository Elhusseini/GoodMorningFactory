// GoodMorningFactory/Core/Services/FinancialReportsService.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class FinancialReportsService : IFinancialReportsService
    {
        public async Task<List<TrialBalanceItemViewModel>> GetTrialBalanceAsync(DateTime toDate)
        {
            // ... (الكود الحالي يبقى كما هو)
            using (var db = new DatabaseContext())
            {
                var accountBalances = await db.JournalVoucherItems
                    .Where(i => i.JournalVoucher.VoucherDate <= toDate)
                    .GroupBy(i => i.Account)
                    .Select(g => new
                    {
                        Account = g.Key,
                        TotalDebit = g.Sum(i => i.Debit),
                        TotalCredit = g.Sum(i => i.Credit)
                    })
                    .ToListAsync();
                var trialBalanceItems = new List<TrialBalanceItemViewModel>();
                foreach (var item in accountBalances)
                {
                    var balance = item.TotalDebit - item.TotalCredit;
                    if (balance == 0) continue;
                    var vm = new TrialBalanceItemViewModel
                    {
                        AccountNumber = item.Account.AccountNumber,
                        AccountName = item.Account.AccountName
                    };
                    if (balance > 0)
                    {
                        vm.DebitBalance = balance;
                        vm.CreditBalance = 0;
                    }
                    else
                    {
                        vm.DebitBalance = 0;
                        vm.CreditBalance = -balance;
                    }
                    trialBalanceItems.Add(vm);
                }
                return trialBalanceItems.OrderBy(i => i.AccountNumber).ToList();
            }
        }

        // --- بداية الإضافة: تنفيذ دالة قائمة الدخل ---
        public async Task<IncomeStatementViewModel> GetIncomeStatementAsync(DateTime fromDate, DateTime toDate)
        {
            var viewModel = new IncomeStatementViewModel();
            viewModel.ReportDateRange = $"للفترة من {fromDate:yyyy/MM/dd} إلى {toDate:yyyy/MM/dd}";

            using (var db = new DatabaseContext())
            {
                // جلب أرصدة حسابات الإيرادات والمصاريف فقط خلال الفترة المحددة
                var accountMovements = await db.JournalVoucherItems
                    .Include(i => i.Account)
                    .Where(i => i.JournalVoucher.VoucherDate >= fromDate && i.JournalVoucher.VoucherDate <= toDate)
                    .Where(i => i.Account.AccountType == AccountType.Revenue || i.Account.AccountType == AccountType.Expense)
                    .GroupBy(i => new { i.Account.Id, i.Account.AccountNumber, i.Account.AccountName, i.Account.AccountType })
                    .Select(g => new
                    {
                        g.Key.AccountNumber,
                        g.Key.AccountName,
                        g.Key.AccountType,
                        Balance = g.Sum(i => i.Credit - i.Debit) // الإيرادات (دائنة) والمصاريف (مدينة)
                    })
                    .ToListAsync();

                // فصل الحسابات إلى إيرادات ومصاريف
                foreach (var movement in accountMovements)
                {
                    if (movement.Balance == 0) continue;

                    var item = new IncomeStatementItemViewModel
                    {
                        AccountNumber = movement.AccountNumber,
                        AccountName = movement.AccountName,
                        Balance = movement.Balance
                    };

                    if (movement.AccountType == AccountType.Revenue)
                    {
                        viewModel.Revenues.Add(item);
                    }
                    else if (movement.AccountType == AccountType.Expense)
                    {
                        viewModel.Expenses.Add(item);
                    }
                }
            }
            // استدعاء دالة الحسابات النهائية
            viewModel.CalculateTotals();
            return viewModel;
        }
        // --- نهاية الإضافة ---
    }
}