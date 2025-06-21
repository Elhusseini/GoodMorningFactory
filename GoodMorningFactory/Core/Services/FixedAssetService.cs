// GoodMorningFactory/Core/Services/FixedAssetService.cs
// *** الكود الكامل والشامل - تم تعديل دالة احتساب الإهلاك ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class FixedAssetService : IFixedAssetService
    {
        // --- دوال المرحلة الأولى (كاملة وبدون اختصار) ---
        public async Task<List<FixedAsset>> GetAssetsAsync()
        {
            using (var db = new DatabaseContext()) { return await db.FixedAssets.OrderByDescending(a => a.AcquisitionDate).ToListAsync(); }
        }
        public async Task<FixedAsset> GetAssetByIdAsync(int id)
        {
            using (var db = new DatabaseContext()) { return await db.FixedAssets.Include(a => a.AssetAccount).Include(a => a.AccumulatedDepreciationAccount).Include(a => a.DepreciationExpenseAccount).FirstOrDefaultAsync(a => a.Id == id); }
        }
        public async Task<List<Account>> GetAssetAccountsAsync()
        {
            using (var db = new DatabaseContext()) { return await db.Accounts.Where(a => a.AccountType == AccountType.Asset).OrderBy(a => a.AccountNumber).ToListAsync(); }
        }
        public async Task<List<Account>> GetExpenseAccountsAsync()
        {
            using (var db = new DatabaseContext()) { return await db.Accounts.Where(a => a.AccountType == AccountType.Expense).OrderBy(a => a.AccountNumber).ToListAsync(); }
        }
        public async Task SaveAssetAsync(FixedAsset asset)
        {
            using (var db = new DatabaseContext()) { if (asset.Id == 0) db.FixedAssets.Add(asset); else db.FixedAssets.Update(asset); await db.SaveChangesAsync(); }
        }
        public async Task DeleteAssetAsync(int id)
        {
            using (var db = new DatabaseContext()) { bool hasDepreciationRecords = await db.AssetDepreciations.AnyAsync(d => d.FixedAssetId == id); if (hasDepreciationRecords) throw new InvalidOperationException("لا يمكن حذف هذا الأصل لأنه يحتوي على سجلات إهلاك. يمكنك التخلص من الأصل بدلاً من حذفه."); var assetToDelete = await db.FixedAssets.FindAsync(id); if (assetToDelete != null) { db.FixedAssets.Remove(assetToDelete); await db.SaveChangesAsync(); } }
        }

        // --- دوال المرحلة الثانية (المعدلة) ---
        public async Task<(JournalVoucher voucher, Dictionary<int, decimal> details)> CalculateDepreciationVoucherAsync(DateTime periodDate)
        {
            using (var db = new DatabaseContext())
            {
                string expectedDescription = $"إهلاك الأصول الثابتة عن شهر {periodDate.Month}/{periodDate.Year}";
                bool alreadyPosted = await db.JournalVouchers.AnyAsync(jv => jv.Description == expectedDescription);
                if (alreadyPosted)
                {
                    throw new InvalidOperationException("تم احتساب وترحيل قيد الإهلاك لهذه الفترة من قبل.");
                }

                var assetsToDepreciate = await db.FixedAssets
                    .Include(a => a.DepreciationExpenseAccount)
                    .Include(a => a.AccumulatedDepreciationAccount)
                    .Where(a => !a.IsDisposed && a.AcquisitionDate <= periodDate)
                    .ToListAsync();

                if (!assetsToDepreciate.Any()) return (null, null);

                var totalDepreciationByExpenseAcc = new Dictionary<int, decimal>();
                var totalDepreciationByAccumulatedAcc = new Dictionary<int, decimal>();
                var assetsDepreciationDetails = new Dictionary<int, decimal>();

                foreach (var asset in assetsToDepreciate)
                {
                    if (asset.UsefulLifeYears <= 0) continue;

                    decimal yearlyDepreciation = (asset.AcquisitionCost - asset.SalvageValue) / asset.UsefulLifeYears;
                    decimal monthlyDepreciation = Math.Round(yearlyDepreciation / 12, 2);

                    decimal previousAccumulated = await db.AssetDepreciations
                        .Where(d => d.FixedAssetId == asset.Id)
                        .SumAsync(d => d.Amount);

                    if ((previousAccumulated + monthlyDepreciation) > (asset.AcquisitionCost - asset.SalvageValue))
                    {
                        monthlyDepreciation = (asset.AcquisitionCost - asset.SalvageValue) - previousAccumulated;
                    }

                    if (monthlyDepreciation > 0)
                    {
                        assetsDepreciationDetails.Add(asset.Id, monthlyDepreciation);

                        if (!totalDepreciationByExpenseAcc.ContainsKey(asset.DepreciationExpenseAccountId))
                            totalDepreciationByExpenseAcc[asset.DepreciationExpenseAccountId] = 0;
                        totalDepreciationByExpenseAcc[asset.DepreciationExpenseAccountId] += monthlyDepreciation;

                        if (!totalDepreciationByAccumulatedAcc.ContainsKey(asset.AccumulatedDepreciationAccountId))
                            totalDepreciationByAccumulatedAcc[asset.AccumulatedDepreciationAccountId] = 0;
                        totalDepreciationByAccumulatedAcc[asset.AccumulatedDepreciationAccountId] += monthlyDepreciation;
                    }
                }

                if (!assetsDepreciationDetails.Any()) return (null, null);

                var proposedVoucher = new JournalVoucher
                {
                    VoucherDate = periodDate,
                    Description = expectedDescription,
                    Status = VoucherStatus.Proposed,
                    JournalVoucherItems = new List<JournalVoucherItem>()
                };

                foreach (var entry in totalDepreciationByExpenseAcc)
                {
                    var account = await db.Accounts.FindAsync(entry.Key);
                    proposedVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = entry.Key, Account = account, Debit = entry.Value });
                }

                foreach (var entry in totalDepreciationByAccumulatedAcc)
                {
                    var account = await db.Accounts.FindAsync(entry.Key);
                    proposedVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = entry.Key, Account = account, Credit = entry.Value });
                }

                proposedVoucher.TotalDebit = proposedVoucher.JournalVoucherItems.Sum(i => i.Debit);
                proposedVoucher.TotalCredit = proposedVoucher.JournalVoucherItems.Sum(i => i.Credit);

                return (proposedVoucher, assetsDepreciationDetails);
            }
        }

        public async Task PostDepreciationVoucherAsync(JournalVoucher proposedVoucher, Dictionary<int, decimal> depreciatedAssets)
        {
            using (var db = new DatabaseContext())
            {
                using (var transaction = await db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        proposedVoucher.Status = VoucherStatus.Posted;
                        proposedVoucher.VoucherNumber = $"DEP-{proposedVoucher.VoucherDate:yyyy-MM}";
                        foreach (var item in proposedVoucher.JournalVoucherItems) item.Account = null;
                        db.JournalVouchers.Add(proposedVoucher);
                        await db.SaveChangesAsync();

                        foreach (var assetEntry in depreciatedAssets)
                        {
                            var depreciationRecord = new AssetDepreciation
                            {
                                FixedAssetId = assetEntry.Key,
                                JournalVoucherId = proposedVoucher.Id,
                                DepreciationDate = proposedVoucher.VoucherDate,
                                Amount = assetEntry.Value
                            };
                            db.AssetDepreciations.Add(depreciationRecord);
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