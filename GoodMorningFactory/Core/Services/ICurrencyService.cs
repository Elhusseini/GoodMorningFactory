// GoodMorningFactory/Core/Services/ICurrencyService.cs
// *** الكود الكامل والمعدل ***
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface ICurrencyService
    {
        // ======================= بداية الإضافة =======================
        /// <summary>
        /// جلب قائمة بجميع العملات.
        /// </summary>
        Task<List<Currency>> GetCurrenciesAsync();
        // ======================== نهاية الإضافة ========================

        Task<Currency> GetCurrencyByIdAsync(int currencyId);
        Task SaveCurrencyAsync(Currency currency);

        // ======================= بداية الإضافة =======================
        /// <summary>
        /// حذف عملة من قاعدة البيانات.
        /// </summary>
        Task DeleteCurrencyAsync(int currencyId);
        // ======================== نهاية الإضافة ========================
    }
}