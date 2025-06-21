// GoodMorningFactory/Core/Services/CurrencyService.cs
// *** الكود الكامل والمعدل ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class CurrencyService : ICurrencyService
    {
        public async Task<List<Currency>> GetCurrenciesAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Currencies.OrderBy(c => c.Name).ToListAsync();
            }
        }

        public async Task<Currency> GetCurrencyByIdAsync(int currencyId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Currencies.FindAsync(currencyId);
            }
        }

        public async Task SaveCurrencyAsync(Currency currency)
        {
            using (var db = new DatabaseContext())
            {
                if (currency.Id == 0)
                {
                    db.Currencies.Add(currency);
                }
                else
                {
                    db.Currencies.Update(currency);
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteCurrencyAsync(int currencyId)
        {
            using (var db = new DatabaseContext())
            {
                var currencyToDelete = await db.Currencies.FindAsync(currencyId);
                if (currencyToDelete != null)
                {
                    db.Currencies.Remove(currencyToDelete);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}