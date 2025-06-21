// GoodMorningFactory/Core/Services/PriceListService.cs
// *** الكود الكامل والشامل لكلاس الخدمة الموحد ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class PriceListService : IPriceListService
    {
        public async Task<List<PriceList>> GetPriceListsAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.PriceLists.OrderBy(p => p.Name).ToListAsync();
            }
        }

        public async Task<PriceList> GetPriceListByIdAsync(int priceListId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.PriceLists
                    .Include(p => p.ProductPrices)
                    .ThenInclude(pp => pp.Product)
                    .FirstOrDefaultAsync(p => p.Id == priceListId);
            }
        }

        public async Task SavePriceListAsync(PriceList priceList)
        {
            using (var db = new DatabaseContext())
            {
                if (priceList.Id == 0)
                {
                    db.PriceLists.Add(priceList);
                }
                else
                {
                    // نستخدم Update لتحديث الكيان بأكمله بناء على الـ Id
                    db.PriceLists.Update(priceList);
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task DeletePriceListAsync(int priceListId)
        {
            using (var db = new DatabaseContext())
            {
                var priceListToDelete = await db.PriceLists.FindAsync(priceListId);
                if (priceListToDelete != null)
                {
                    db.PriceLists.Remove(priceListToDelete);
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task<List<Product>> GetAvailableProductsAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Products
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
        }

        public async Task SaveProductPricesAsync(int priceListId, IEnumerable<ProductPrice> updatedProductPrices)
        {
            using (var db = new DatabaseContext())
            {
                var existingPrices = await db.ProductPrices
                                             .Where(p => p.PriceListId == priceListId)
                                             .ToListAsync();

                var pricesToDelete = existingPrices
                    .Where(ep => !updatedProductPrices.Any(up => up.ProductId == ep.ProductId))
                    .ToList();
                db.ProductPrices.RemoveRange(pricesToDelete);

                foreach (var updatedPrice in updatedProductPrices)
                {
                    var existingPrice = existingPrices.FirstOrDefault(p => p.ProductId == updatedPrice.ProductId);
                    if (existingPrice != null)
                    {
                        existingPrice.Price = updatedPrice.Price;
                    }
                    else
                    {
                        db.ProductPrices.Add(new ProductPrice
                        {
                            PriceListId = priceListId,
                            ProductId = updatedPrice.ProductId,
                            Price = updatedPrice.Price
                        });
                    }
                }
                await db.SaveChangesAsync();
            }
        }
    }
}