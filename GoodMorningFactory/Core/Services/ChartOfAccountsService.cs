// GoodMorningFactory/Core/Services/ChartOfAccountsService.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class ChartOfAccountsService : IChartOfAccountsService
    {
        public async Task<ObservableCollection<AccountViewModel>> GetAccountTreeAsync()
        {
            using (var db = new DatabaseContext())
            {
                var allAccounts = await db.Accounts.OrderBy(a => a.AccountNumber).ToListAsync();
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
                return rootNodes;
            }
        }

        public async Task<List<LedgerEntryViewModel>> GetLedgerEntriesAsync(int accountId)
        {
            using (var db = new DatabaseContext())
            {
                var ledgerEntries = await db.JournalVoucherItems
                    .Include(i => i.JournalVoucher)
                    .Where(i => i.AccountId == accountId)
                    .OrderBy(i => i.JournalVoucher.VoucherDate)
                    .ToListAsync();

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
                return ledgerViewModels;
            }
        }

        public async Task DeleteAccountAsync(int accountId)
        {
            using (var db = new DatabaseContext())
            {
                if (await db.Accounts.AnyAsync(a => a.ParentAccountId == accountId))
                {
                    throw new InvalidOperationException("لا يمكن حذف هذا الحساب لأنه يحتوي على حسابات فرعية.");
                }

                if (await db.JournalVoucherItems.AnyAsync(i => i.AccountId == accountId))
                {
                    throw new InvalidOperationException("لا يمكن حذف هذا الحساب لوجود حركات مرتبطة به. يمكنك جعله 'غير نشط'.");
                }

                var accountToDelete = await db.Accounts.FindAsync(accountId);
                if (accountToDelete != null)
                {
                    db.Accounts.Remove(accountToDelete);
                    await db.SaveChangesAsync();
                }
            }
        }

        // --- بداية الإضافة: تنفيذ الدوال الجديدة ---
        public async Task<AddEditAccountDto> GetDataForAddEditAccountAsync(int? accountId)
        {
            using (var db = new DatabaseContext())
            {
                var dto = new AddEditAccountDto();

                // جلب الحساب للتعديل أو إنشاء حساب جديد
                if (accountId.HasValue)
                {
                    dto.Account = await db.Accounts.FindAsync(accountId.Value);
                }
                else
                {
                    dto.Account = new Account { IsActive = true };
                }

                // جلب كل الحسابات المحتملة لتكون حساب أب
                // نستثني الحساب الحالي وأبناءه من القائمة
                dto.AllAccounts = await db.Accounts
                    .Where(a => a.Id != accountId)
                    .OrderBy(a => a.AccountNumber)
                    .ToListAsync();

                return dto;
            }
        }

        public async Task SaveAccountAsync(Account account)
        {
            using (var db = new DatabaseContext())
            {
                if (account.Id == 0) // إضافة حساب جديد
                {
                    db.Accounts.Add(account);
                }
                else // تعديل حساب موجود
                {
                    db.Accounts.Update(account);
                }
                await db.SaveChangesAsync();
            }
        }
        // --- نهاية الإضافة ---
    }
}