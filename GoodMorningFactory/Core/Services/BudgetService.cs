// GoodMorningFactory/Core/Services/BudgetService.cs
// *** الكود الكامل والمصحح - تم تحسين منطق الحفظ للتعديل ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class BudgetService : IBudgetService
    {
        public async Task<List<Budget>> GetBudgetsAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Budgets
                    .OrderByDescending(b => b.Year)
                    .ThenBy(b => b.Name)
                    .ToListAsync();
            }
        }

        public async Task<Budget> GetBudgetWithDetailsAsync(int budgetId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Budgets
                    .Include(b => b.BudgetDetails)
                    .ThenInclude(bd => bd.Account)
                    .FirstOrDefaultAsync(b => b.Id == budgetId);
            }
        }

        public async Task<List<Account>> GetBudgetableAccountsAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Accounts
                    .Where(a => a.AccountType == AccountType.Revenue || a.AccountType == AccountType.Expense)
                    .OrderBy(a => a.AccountNumber)
                    .ToListAsync();
            }
        }

        public async Task SaveBudgetAsync(Budget budgetFromUi)
        {
            using (var db = new DatabaseContext())
            {
                if (budgetFromUi.Id == 0) // الحالة 1: إضافة موازنة جديدة
                {
                    db.Budgets.Add(budgetFromUi);
                }
                else // الحالة 2: تعديل موازنة موجودة (هنا الإصلاح)
                {
                    var existingBudgetInDb = await db.Budgets
                        .Include(b => b.BudgetDetails)
                        .FirstOrDefaultAsync(b => b.Id == budgetFromUi.Id);

                    if (existingBudgetInDb == null)
                    {
                        throw new Exception("لم يتم العثور على الموازنة لتعديلها.");
                    }

                    db.Entry(existingBudgetInDb).CurrentValues.SetValues(budgetFromUi);

                    foreach (var detailFromUi in budgetFromUi.BudgetDetails)
                    {
                        var existingDetail = existingBudgetInDb.BudgetDetails
                            .FirstOrDefault(d => d.AccountId == detailFromUi.AccountId);

                        if (existingDetail != null)
                        {
                            existingDetail.Month1Amount = detailFromUi.Month1Amount;
                            existingDetail.Month2Amount = detailFromUi.Month2Amount;
                            existingDetail.Month3Amount = detailFromUi.Month3Amount;
                            existingDetail.Month4Amount = detailFromUi.Month4Amount;
                            existingDetail.Month5Amount = detailFromUi.Month5Amount;
                            existingDetail.Month6Amount = detailFromUi.Month6Amount;
                            existingDetail.Month7Amount = detailFromUi.Month7Amount;
                            existingDetail.Month8Amount = detailFromUi.Month8Amount;
                            existingDetail.Month9Amount = detailFromUi.Month9Amount;
                            existingDetail.Month10Amount = detailFromUi.Month10Amount;
                            existingDetail.Month11Amount = detailFromUi.Month11Amount;
                            existingDetail.Month12Amount = detailFromUi.Month12Amount;
                        }
                        else
                        {
                            existingBudgetInDb.BudgetDetails.Add(detailFromUi);
                        }
                    }
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteBudgetAsync(int budgetId)
        {
            using (var db = new DatabaseContext())
            {
                var budgetToDelete = await db.Budgets.FindAsync(budgetId);
                if (budgetToDelete != null)
                {
                    db.Budgets.Remove(budgetToDelete);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}