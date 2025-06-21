// GoodMorningFactory/Core/Services/SalesQuotationService.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// خدمة مركزية لإدارة العمليات المتعلقة بعروض الأسعار.
    /// </summary>
    public class SalesQuotationService : ISalesQuotationService
    {
        public async Task<PaginatedResult<SalesQuotation>> GetQuotationsAsync(QuotationFilterCriteria criteria)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.SalesQuotations.Include(q => q.Customer).AsQueryable();

                // تطبيق الفلاتر
                if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                {
                    string searchTextLower = criteria.SearchText.ToLower();
                    query = query.Where(q => q.QuotationNumber.ToLower().Contains(searchTextLower) || q.Customer.CustomerName.ToLower().Contains(searchTextLower));
                }
                if (criteria.Status.HasValue)
                {
                    query = query.Where(q => q.Status == criteria.Status.Value);
                }
                if (criteria.FromDate.HasValue)
                {
                    query = query.Where(q => q.QuotationDate.Date >= criteria.FromDate.Value.Date);
                }
                if (criteria.ToDate.HasValue)
                {
                    query = query.Where(q => q.QuotationDate.Date <= criteria.ToDate.Value.Date);
                }

                var totalItems = await query.CountAsync();

                var items = await query.OrderByDescending(q => q.QuotationDate)
                                       .Skip((criteria.Page - 1) * criteria.PageSize)
                                       .Take(criteria.PageSize)
                                       .ToListAsync();

                return new PaginatedResult<SalesQuotation> { Items = items, TotalCount = totalItems };
            }
        }

        public async Task DeleteQuotationAsync(int quotationId)
        {
            using (var db = new DatabaseContext())
            {
                var quotationToDelete = await db.SalesQuotations
                                                .Include(q => q.SalesQuotationItems)
                                                .FirstOrDefaultAsync(q => q.Id == quotationId);
                if (quotationToDelete != null)
                {
                    // يجب حذف البنود المرتبطة أولاً
                    db.SalesQuotationItems.RemoveRange(quotationToDelete.SalesQuotationItems);
                    db.SalesQuotations.Remove(quotationToDelete);
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task UpdateQuotationStatusAsync(int quotationId, QuotationStatus newStatus)
        {
            using (var db = new DatabaseContext())
            {
                var quotation = await db.SalesQuotations.FindAsync(quotationId);
                if (quotation != null)
                {
                    quotation.Status = newStatus;
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}