// GoodMorningFactory/Services/CrmService.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Services
{
    public class CrmService : ICrmService
    {
        private DatabaseContext CreateDbContext() => new DatabaseContext();

        // --- عمليات العملاء المحتملين (Leads) ---
        public async Task<List<Lead>> GetLeadsAsync()
        {
            using (var db = CreateDbContext())
            {
                return await db.Leads
                                 .Include(l => l.AssignedToUser)
                                 .OrderByDescending(l => l.CreatedDate)
                                 .ToListAsync();
            }
        }

        public async Task AddLeadAsync(Lead newLead)
        {
            using (var db = CreateDbContext())
            {
                db.Leads.Add(newLead);
                await db.SaveChangesAsync();
            }
        }

        public async Task ConvertLeadToCustomerAsync(Lead leadToConvert)
        {
            using (var db = CreateDbContext())
            {
                var newCustomer = new Customer
                {
                    CustomerCode = $"CUST-{await db.Customers.CountAsync() + 1}",
                    CustomerName = leadToConvert.CompanyName,
                    ContactPerson = leadToConvert.ContactPerson,
                    Email = leadToConvert.Email,
                    PhoneNumber = leadToConvert.PhoneNumber,
                    BillingAddress = "N/A",
                    ShippingAddress = "N/A"
                };
                db.Customers.Add(newCustomer);

                var leadInDb = await db.Leads.FindAsync(leadToConvert.Id);
                if (leadInDb != null)
                {
                    db.Leads.Remove(leadInDb);
                }

                await db.SaveChangesAsync();
            }
        }

        // --- عمليات الفرص البيعية (Opportunities) ---
        public async Task<List<Opportunity>> GetOpportunitiesAsync()
        {
            using (var db = CreateDbContext())
            {
                return await db.Opportunities
                                 .Include(o => o.Customer)
                                 .Include(o => o.AssignedToUser)
                                 .OrderByDescending(o => o.CloseDate)
                                 .ToListAsync();
            }
        }

        public async Task SaveOpportunityAsync(Opportunity opportunity)
        {
            using (var db = CreateDbContext())
            {
                if (opportunity.Id > 0)
                {
                    db.Opportunities.Update(opportunity);
                }
                else
                {
                    db.Opportunities.Add(opportunity);
                }
                await db.SaveChangesAsync();
            }
        }

        // --- بداية الإضافة: تنفيذ دالة الحذف ---
        public async Task DeleteOpportunityAsync(int opportunityId)
        {
            using (var db = CreateDbContext())
            {
                var opportunityToDelete = await db.Opportunities.FindAsync(opportunityId);
                if (opportunityToDelete != null)
                {
                    db.Opportunities.Remove(opportunityToDelete);
                    await db.SaveChangesAsync();
                }
            }
        }
        // --- نهاية الإضافة ---

        // --- عمليات مشتركة ---
        public async Task<List<Customer>> GetActiveCustomersAsync()
        {
            using (var db = CreateDbContext())
            {
                return await db.Customers.Where(c => c.IsActive).ToListAsync();
            }
        }

        public async Task<List<User>> GetActiveUsersAsync()
        {
            using (var db = CreateDbContext())
            {
                return await db.Users.Where(u => u.IsActive).ToListAsync();
            }
        }
    }
}