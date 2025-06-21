// GoodMorningFactory/Core/Services/SalesDashboardService.cs
// *** ملف جديد: التنفيذ الفعلي لخدمة لوحة معلومات المبيعات ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class SalesDashboardService : ISalesDashboardService
    {
        public async Task<SalesDashboardDto> GetDashboardDataAsync()
        {
            using (var db = new DatabaseContext())
            {
                var today = DateTime.Today;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                var startOfYear = new DateTime(today.Year, 1, 1);

                // جلب البيانات المطلوبة في استعلامات قليلة
                var salesThisMonth = await db.Sales
                    .Where(s => s.SaleDate >= startOfMonth && s.SaleDate < startOfMonth.AddMonths(1))
                    .ToListAsync();

                // حساب مؤشرات الأداء الرئيسية
                var totalSales = salesThisMonth.Sum(s => s.TotalAmount);
                var newOrders = await db.SalesOrders.CountAsync(o => o.OrderDate >= startOfMonth && o.OrderDate < startOfMonth.AddMonths(1));
                var followUpQuotes = await db.SalesQuotations.CountAsync(q => q.Status == QuotationStatus.Sent && q.ValidUntilDate >= today);
                var avgOrderValue = salesThisMonth.Any() ? salesThisMonth.Average(s => s.TotalAmount) : 0;

                // ======================= بداية الإصلاح الرئيسي =======================
                // جلب البيانات المجمعة أولاً إلى الذاكرة
                var customerSalesData = await db.Sales.Include(s => s.Customer)
                    .Where(s => s.SaleDate.Year == today.Year && s.Customer != null)
                    .GroupBy(s => s.Customer.CustomerName)
                    .Select(g => new { Name = g.Key, Total = g.Sum(s => s.TotalAmount) })
                    .ToListAsync();

                // ثم القيام بالترتيب في الذاكرة
                var topCustomers = customerSalesData
                    .OrderByDescending(x => x.Total)
                    .Take(5)
                    .Select(x => x.Name)
                    .ToList();
                // ======================== نهاية الإصلاح الرئيسي ========================

                var topProducts = await db.SaleItems.Include(si => si.Product)
                    .Where(si => si.Sale.SaleDate.Year == today.Year)
                    .GroupBy(si => si.Product.Name)
                    .Select(g => new { Name = g.Key, Quantity = g.Sum(si => si.Quantity) })
                    .OrderByDescending(x => x.Quantity).Take(5)
                    .Select(x => $"{x.Name} ({x.Quantity} قطعة)").ToListAsync();

                // حساب قمع المبيعات
                var quotationsCount = await db.SalesQuotations.CountAsync(q => q.QuotationDate >= startOfMonth && q.QuotationDate < startOfMonth.AddMonths(1));
                var invoicesCount = salesThisMonth.Count;

                // حساب المبيعات الشهرية لآخر 6 أشهر
                var monthlySales = new Dictionary<string, decimal>();
                for (int i = 5; i >= 0; i--)
                {
                    var date = DateTime.Now.AddMonths(-i);
                    var firstDay = new DateTime(date.Year, date.Month, 1);
                    var lastDay = firstDay.AddMonths(1);
                    var monthlyTotal = await db.Sales.Where(s => s.SaleDate >= firstDay && s.SaleDate < lastDay).SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
                    monthlySales.Add(firstDay.ToString("MMM yy", new CultureInfo("ar-EG")), monthlyTotal);
                }

                // حساب المبيعات حسب الفئة
                var salesByCategory = await db.SaleItems.Include(si => si.Product.Category)
                    .Where(si => si.Sale.SaleDate.Year == today.Year)
                    .GroupBy(si => si.Product.Category.Name ?? "غير مصنف")
                    .Select(g => new { CategoryName = g.Key, Total = g.Sum(si => si.Quantity * si.UnitPrice) })
                    .ToDictionaryAsync(x => x.CategoryName, x => x.Total);

                return new SalesDashboardDto
                {
                    TotalSalesThisMonth = totalSales,
                    NewOrdersThisMonth = newOrders,
                    FollowUpQuotationsCount = followUpQuotes,
                    AverageOrderValue = avgOrderValue,
                    TopCustomers = topCustomers,
                    TopProducts = topProducts,
                    QuotationsCount = quotationsCount,
                    OrdersCount = newOrders,
                    InvoicesCount = invoicesCount,
                    MonthlySales = monthlySales,
                    SalesByCategory = salesByCategory
                };
            }
        }

        private string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }
    }
}