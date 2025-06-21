using GoodMorningFactory.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// خدمة مركزية مسؤولة عن جلب وتجميع بيانات لوحة التحكم الرئيسية.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        public async Task<decimal> GetTotalSalesTodayAsync()
        {
            using (var db = new DatabaseContext())
            {
                var today = DateTime.Today;
                return await db.Sales
                    .Where(s => s.SaleDate >= today)
                    .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            }
        }

        public async Task<decimal> GetTotalSalesThisMonthAsync()
        {
            using (var db = new DatabaseContext())
            {
                var today = DateTime.Today;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                return await db.Sales
                    .Where(s => s.SaleDate >= startOfMonth)
                    .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            }
        }

        public async Task<int> GetTotalProductsAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Products.CountAsync();
            }
        }

        public async Task<int> GetLowStockProductsCountAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Inventories
                    .CountAsync(i => i.ReorderLevel > 0 && i.Quantity <= i.ReorderLevel);
            }
        }

        public async Task<Dictionary<string, decimal>> GetMonthlySalesDataAsync()
        {
            using (var db = new DatabaseContext())
            {
                var salesData = new Dictionary<string, decimal>();
                for (int i = 5; i >= 0; i--)
                {
                    var date = DateTime.Now.AddMonths(-i);
                    var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                    var monthlyTotal = await db.Sales
                        .Where(s => s.SaleDate >= firstDayOfMonth && s.SaleDate <= lastDayOfMonth)
                        .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

                    string monthLabel = firstDayOfMonth.ToString("MMM yy", new CultureInfo("ar-EG"));
                    salesData.Add(monthLabel, monthlyTotal);
                }
                return salesData;
            }
        }

        public async Task<List<TopSellingProductDto>> GetTopSellingProductsAsync(int count)
        {
            using (var db = new DatabaseContext())
            {
                return await db.SaleItems
                    .Include(si => si.Product)
                    .GroupBy(si => si.Product.Name)
                    .Select(g => new TopSellingProductDto { ProductName = g.Key, TotalQuantity = g.Sum(si => si.Quantity) })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(count)
                    .ToListAsync();
            }
        }
    }
}
