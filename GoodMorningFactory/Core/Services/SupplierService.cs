// GoodMorningFactory/Core/Services/SupplierService.cs
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
    public class SupplierService : ISupplierService
    {
        public async Task<PaginatedResult<SupplierViewModel>> GetSuppliersAsync(SupplierFilterCriteria criteria)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.Suppliers.AsQueryable();

                if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                {
                    string searchTextLower = criteria.SearchText.ToLower();
                    query = query.Where(s => s.Name.ToLower().Contains(searchTextLower) || s.SupplierCode.ToLower().Contains(searchTextLower));
                }

                if (criteria.IsActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == criteria.IsActive.Value);
                }

                int totalItems = await query.CountAsync();

                var suppliers = await query.OrderBy(s => s.Name)
                                           .Skip((criteria.Page - 1) * criteria.PageSize)
                                           .Take(criteria.PageSize)
                                           .Select(s => new SupplierViewModel
                                           {
                                               Id = s.Id,
                                               SupplierCode = s.SupplierCode,
                                               Name = s.Name,
                                               ContactPerson = s.ContactPerson,
                                               PhoneNumber = s.PhoneNumber,
                                               IsActive = s.IsActive,
                                               CurrentBalance = s.Purchases.Sum(p => p.TotalAmount - p.AmountPaid) - s.Purchases.SelectMany(p => p.PurchaseReturns).Sum(pr => pr.TotalReturnValue)
                                           })
                                           .ToListAsync();

                return new PaginatedResult<SupplierViewModel>
                {
                    Items = suppliers,
                    TotalCount = totalItems
                };
            }
        }

        public async Task<Supplier> GetSupplierByIdAsync(int supplierId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Suppliers.FindAsync(supplierId);
            }
        }

        public async Task AddSupplierAsync(Supplier supplier)
        {
            using (var db = new DatabaseContext())
            {
                db.Suppliers.Add(supplier);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateSupplierAsync(Supplier supplier)
        {
            using (var db = new DatabaseContext())
            {
                db.Suppliers.Update(supplier);
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteSupplierAsync(int supplierId)
        {
            using (var db = new DatabaseContext())
            {
                bool hasPurchases = await db.Purchases.AnyAsync(p => p.SupplierId == supplierId);
                if (hasPurchases)
                {
                    throw new InvalidOperationException("لا يمكن حذف المورد لوجود فواتير مشتريات مرتبطة به.");
                }

                var supplier = await db.Suppliers.FindAsync(supplierId);
                if (supplier != null)
                {
                    db.Suppliers.Remove(supplier);
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task<string> GetNextSupplierCodeAsync()
        {
            using (var db = new DatabaseContext())
            {
                var lastSupplier = await db.Suppliers.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
                int nextId = (lastSupplier?.Id ?? 0) + 1;
                return $"SUP-{nextId:D5}";
            }
        }

        public async Task<List<SupplierStatementItemViewModel>> GetSupplierStatementAsync(int supplierId)
        {
            using (var db = new DatabaseContext())
            {
                var supplier = await db.Suppliers.FindAsync(supplierId);
                if (supplier == null) return new List<SupplierStatementItemViewModel>();

                var purchases = await db.Purchases
                    .Where(p => p.SupplierId == supplierId)
                    .Select(p => new { Date = p.PurchaseDate, Type = "فاتورة شراء", Ref = p.InvoiceNumber, Debit = 0m, Credit = p.TotalAmount })
                    .ToListAsync();

                var returns = await db.PurchaseReturns
                    .Where(pr => pr.Purchase.SupplierId == supplierId)
                    .Select(pr => new { Date = pr.ReturnDate, Type = "مرتجع مشتريات", Ref = pr.ReturnNumber, Debit = pr.TotalReturnValue, Credit = 0m })
                    .ToListAsync();

                var allTransactions = purchases.Union(returns).OrderBy(t => t.Date).ToList();

                var statementItems = new List<SupplierStatementItemViewModel>();
                decimal currentBalance = 0;
                foreach (var trans in allTransactions)
                {
                    currentBalance += trans.Credit - trans.Debit;
                    statementItems.Add(new SupplierStatementItemViewModel
                    {
                        Date = trans.Date,
                        TransactionType = trans.Type,
                        ReferenceNumber = trans.Ref,
                        Debit = trans.Debit,
                        Credit = trans.Credit,
                        Balance = currentBalance
                    });
                }
                return statementItems;
            }
        }
    }
}
