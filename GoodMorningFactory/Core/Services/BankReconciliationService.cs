// GoodMorningFactory/Core/Services/BankReconciliationService.cs
// *** الكود الكامل لكلاس خدمة التسوية البنكية ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class BankReconciliationService : IBankReconciliationService
    {
        public async Task<List<Account>> GetBankAccountsAsync()
        {
            using (var db = new DatabaseContext())
            {
                // نفترض وجود خاصية IsBank في نموذج الحساب للتمييز
                return await db.Accounts.Where(a => a.AccountType == AccountType.Asset && a.IsBank).OrderBy(a => a.AccountName).ToListAsync();
            }
        }

        // بقية الدوال في الخدمة تبقى كما هي في الرد السابق (لا تتأثر بتعديل نموذج الحساب)
        public async Task<List<JournalVoucherItem>> GetUnreconciledTransactionsAsync(int bankAccountId, DateTime toDate)
        {
            using (var db = new DatabaseContext())
            {
                return await db.JournalVoucherItems
                    .Include(i => i.JournalVoucher)
                    .Where(i => i.AccountId == bankAccountId && !i.IsReconciled && i.JournalVoucher.VoucherDate <= toDate)
                    .OrderBy(i => i.JournalVoucher.VoucherDate)
                    .ToListAsync();
            }
        }

        public async Task<decimal> GetBookBalanceAsync(int bankAccountId, DateTime toDate)
        {
            using (var db = new DatabaseContext())
            {
                // Calculate balance from all transactions up to the statement date
                return await db.JournalVoucherItems
                    .Where(i => i.AccountId == bankAccountId && i.JournalVoucher.VoucherDate <= toDate)
                    .SumAsync(i => i.Debit - i.Credit);
            }
        }

        public async Task SaveReconciliationAsync(BankReconciliation reconciliation, List<int> reconciledItemIds)
        {
            using (var db = new DatabaseContext())
            {
                using (var transaction = await db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Save the main reconciliation record to get its ID
                        db.BankReconciliations.Add(reconciliation);
                        await db.SaveChangesAsync();

                        // 2. Update the selected journal items
                        var itemsToUpdate = await db.JournalVoucherItems
                            .Where(i => reconciledItemIds.Contains(i.Id))
                            .ToListAsync();

                        foreach (var item in itemsToUpdate)
                        {
                            item.IsReconciled = true;
                            item.ReconciliationDate = reconciliation.StatementDate;
                            item.BankReconciliationId = reconciliation.Id;
                        }

                        await db.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }
    }
}