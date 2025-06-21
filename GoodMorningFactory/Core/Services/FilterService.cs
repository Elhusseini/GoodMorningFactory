using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.Core.Helpers;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// خدمة مركزية مسؤولة عن جلب البيانات المستخدمة في فلاتر الواجهات.
    /// </summary>
    public class FilterService : IFilterService
    {
        public async Task<List<FilterItem<int>>> GetCategoryFiltersAsync()
        {
            using (var db = new DatabaseContext())
            {
                var categories = await db.Categories
                                         .OrderBy(c => c.Name)
                                         .Select(c => new FilterItem<int> { Name = c.Name, Value = c.Id })
                                         .ToListAsync();
                categories.Insert(0, new FilterItem<int> { Name = "الكل", Value = 0 });
                return categories;
            }
        }

        public async Task<List<FilterItem<int>>> GetSupplierFiltersAsync()
        {
            using (var db = new DatabaseContext())
            {
                var suppliers = await db.Suppliers
                                        .Where(s => s.IsActive)
                                        .OrderBy(s => s.Name)
                                        .Select(s => new FilterItem<int> { Name = s.Name, Value = s.Id })
                                        .ToListAsync();
                suppliers.Insert(0, new FilterItem<int> { Name = "الكل", Value = 0 });
                return suppliers;
            }
        }

        public List<FilterItem<ProductType?>> GetProductTypeFilters()
        {
            var types = new List<FilterItem<ProductType?>> { new FilterItem<ProductType?> { Name = "الكل", Value = null } };
            types.AddRange(Enum.GetValues(typeof(ProductType))
                               .Cast<ProductType>()
                               .Select(pt => new FilterItem<ProductType?> { Name = pt.GetDescription(), Value = pt }));
            return types;
        }

        public List<FilterItem<bool?>> GetStatusFilters()
        {
            return new List<FilterItem<bool?>>
            {
                new FilterItem<bool?> { Name = "الكل", Value = null },
                new FilterItem<bool?> { Name = "نشط", Value = true },
                new FilterItem<bool?> { Name = "غير نشط", Value = false }
            };
        }

        public CompanyInfo GetCompanyInfo()
        {
            using (var db = new DatabaseContext())
            {
                return db.CompanyInfos.FirstOrDefault();
            }
        }

        public async Task<List<PriceList>> GetPriceListsAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.PriceLists.ToListAsync();
            }
        }

        public List<FilterItem<OrderStatus?>> GetOrderStatusFilters()
        {
            var statuses = new List<FilterItem<OrderStatus?>> { new FilterItem<OrderStatus?> { Name = "الكل", Value = null } };
            statuses.AddRange(Enum.GetValues(typeof(OrderStatus))
                                  .Cast<OrderStatus>()
                                  .Select(s => new FilterItem<OrderStatus?> { Name = s.GetDescription(), Value = s }));
            return statuses;
        }

        /// <summary>
        /// تطبيق الدالة الجديدة لجلب قائمة حالات الشحن.
        /// </summary>
        public List<FilterItem<ShipmentStatus?>> GetShipmentStatusFilters()
        {
            var statuses = new List<FilterItem<ShipmentStatus?>> { new FilterItem<ShipmentStatus?> { Name = "الكل", Value = null } };
            statuses.AddRange(Enum.GetValues(typeof(ShipmentStatus))
                                  .Cast<ShipmentStatus>()
                                  .Select(s => new FilterItem<ShipmentStatus?> { Name = s.GetDescription(), Value = s }));
            return statuses;
        }
    }
}
