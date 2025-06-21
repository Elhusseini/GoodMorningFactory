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
    public class JournalVoucherService : IJournalVoucherService
    {
        public async Task<List<JournalVoucherViewModel>> GetVouchersAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.JournalVouchers
                    .OrderByDescending(jv => jv.VoucherDate)
                    .Select(jv => new JournalVoucherViewModel
                    {
                        Id = jv.Id,
                        VoucherNumber = jv.VoucherNumber,
                        VoucherDate = jv.VoucherDate,
                        Description = jv.Description,
                        TotalDebit = jv.TotalDebit,
                        TotalCredit = jv.TotalCredit,
                        Status = jv.Status
                    })
                    .ToListAsync();
            }
        }

        public async Task ReverseVoucherAsync(int voucherId)
        {
            using (var db = new DatabaseContext())
            {
                var originalVoucher = await db.JournalVouchers
                    .Include(jv => jv.JournalVoucherItems)
                    .FirstOrDefaultAsync(jv => jv.Id == voucherId);

                if (originalVoucher == null || originalVoucher.IsReversed)
                {
                    throw new InvalidOperationException("لا يمكن عكس هذا القيد (قد يكون تم عكسه من قبل).");
                }

                var reversedVoucher = new JournalVoucher
                {
                    VoucherNumber = $"REV-{originalVoucher.VoucherNumber}",
                    VoucherDate = DateTime.Today,
                    Description = $"قيد عكسي للقيد رقم: {originalVoucher.VoucherNumber}",
                    TotalDebit = originalVoucher.TotalCredit,
                    TotalCredit = originalVoucher.TotalDebit,
                    Status = VoucherStatus.Posted
                };

                foreach (var item in originalVoucher.JournalVoucherItems)
                {
                    reversedVoucher.JournalVoucherItems.Add(new JournalVoucherItem
                    {
                        AccountId = item.AccountId,
                        Debit = item.Credit,
                        Credit = item.Debit,
                        Description = item.Description,
                        CostCenterId = item.CostCenterId
                    });
                }

                originalVoucher.IsReversed = true;
                db.JournalVouchers.Add(reversedVoucher);
                await db.SaveChangesAsync();
            }
        }


        // --- بداية التعديل: تحديث منطق جلب البيانات ---
        public async Task<AddEditJournalVoucherDto> GetInitialDataForAddEditWindowAsync()
        {
            using (var db = new DatabaseContext())
            {
                var accounts = await db.Accounts
                                       .OrderBy(a => a.AccountNumber)
                                       .Select(a => new AccountViewModel(a)) // تحويل كل حساب إلى ViewModel
                                       .ToListAsync();

                return new AddEditJournalVoucherDto
                {
                    Accounts = accounts,
                    CostCenters = await db.CostCenters.Where(c => c.IsActive).ToListAsync()
                };
            }
        }
        // --- نهاية التعديل ---
        public async Task SaveVoucherAsync(JournalVoucher voucher)
        {
            using (var db = new DatabaseContext())
            {
                // التحقق من الفترة المحاسبية
                var period = await db.AccountingPeriods.FirstOrDefaultAsync(p => p.Year == voucher.VoucherDate.Year && p.Month == voucher.VoucherDate.Month);
                if (period != null && period.Status == PeriodStatus.Closed)
                {
                    throw new InvalidOperationException("لا يمكن تسجيل القيد في فترة محاسبية مغلقة.");
                }

                db.JournalVouchers.Add(voucher);
                await db.SaveChangesAsync();
            }
        }
        // --- نهاية الإضافة ---
    }
}