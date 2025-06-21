// GoodMorningFactory/Services/ICrmService.cs
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Services
{
    public interface ICrmService
    {
        // العمليات الخاصة بالعملاء المحتملين (Leads)
        Task<List<Lead>> GetLeadsAsync();
        Task AddLeadAsync(Lead newLead);
        Task ConvertLeadToCustomerAsync(Lead leadToConvert);

        // العمليات الخاصة بالفرص البيعية (Opportunities)
        Task<List<Opportunity>> GetOpportunitiesAsync();
        Task SaveOpportunityAsync(Opportunity opportunity);

        // --- بداية الإضافة ---
        /// <summary>
        /// حذف فرصة بيعية من قاعدة البيانات
        /// </summary>
        Task DeleteOpportunityAsync(int opportunityId);
        // --- نهاية الإضافة ---

        // عمليات مشتركة
        Task<List<Customer>> GetActiveCustomersAsync();
        Task<List<User>> GetActiveUsersAsync();
    }
}