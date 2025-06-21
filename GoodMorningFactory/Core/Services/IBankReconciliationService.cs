// GoodMorningFactory/Core/Services/IBankReconciliationService.cs
// *** ملف جديد: واجهة خدمة لوحدة التسوية البنكية ***
using GoodMorningFactory.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IBankReconciliationService
    {
        /// <summary>
        /// جلب جميع حسابات البنوك.
        /// </summary>
        Task<List<Account>> GetBankAccountsAsync();

        /// <summary>
        /// جلب الحركات التي لم تتم تسويتها لحساب بنكي معين حتى تاريخ محدد.
        /// </summary>
        Task<List<JournalVoucherItem>> GetUnreconciledTransactionsAsync(int bankAccountId, DateTime toDate);

        /// <summary>
        /// حساب رصيد الدفاتر لحساب معين في تاريخ محدد.
        /// </summary>
        Task<decimal> GetBookBalanceAsync(int bankAccountId, DateTime toDate);

        /// <summary>
        /// حفظ التسوية عن طريق تحديث الحركات وإنشاء سجل تسوية.
        /// </summary>
        Task SaveReconciliationAsync(BankReconciliation reconciliation, List<int> reconciledItemIds);
    }
}