using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public class StockMovementService : IStockMovementService
    {
        public async Task<PaginatedResult<StockMovementViewModel>> GetStockMovementsAsync(StockMovementFilterCriteria criteria)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.StockMovements
                                .Include(m => m.Product)
                                .Include(m => m.StorageLocation.Warehouse)
                                .Include(m => m.User)
                                .AsQueryable();

                if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                {
                    string searchText = criteria.SearchText.ToLower();
                    query = query.Where(m => m.ReferenceDocument.ToLower().Contains(searchText) || m.Product.Name.ToLower().Contains(searchText));
                }
                if (criteria.MovementType.HasValue)
                {
                    query = query.Where(m => m.MovementType == criteria.MovementType.Value);
                }
                if (criteria.ProductId.HasValue && criteria.ProductId > 0)
                {
                    query = query.Where(m => m.ProductId == criteria.ProductId.Value);
                }
                if (criteria.WarehouseId.HasValue && criteria.WarehouseId > 0)
                {
                    query = query.Where(m => m.StorageLocation.WarehouseId == criteria.WarehouseId.Value);
                }
                if (criteria.FromDate.HasValue)
                {
                    query = query.Where(m => m.MovementDate.Date >= criteria.FromDate.Value.Date);
                }
                if (criteria.ToDate.HasValue)
                {
                    query = query.Where(m => m.MovementDate.Date <= criteria.ToDate.Value.Date);
                }

                int totalItems = await query.CountAsync();

                var movements = await query.OrderByDescending(m => m.MovementDate)
                                         .Skip((criteria.Page - 1) * criteria.PageSize)
                                         .Take(criteria.PageSize)
                                         .ToListAsync();

                var viewModels = movements.Select(m => new StockMovementViewModel
                {
                    Date = m.MovementDate,
                    MovementType = m.MovementType,
                    ReferenceNumber = m.ReferenceDocument,
                    ProductName = m.Product.Name,
                    WarehouseName = m.StorageLocation.Warehouse.Name,
                    StorageLocationName = m.StorageLocation.Name,
                    QuantityIn = new[] { StockMovementType.PurchaseReceipt, StockMovementType.FinishedGoodsProduction, StockMovementType.AdjustmentIncrease, StockMovementType.TransferIn, StockMovementType.SalesReturn }.Contains(m.MovementType) ? m.Quantity : 0,
                    QuantityOut = new[] { StockMovementType.SalesShipment, StockMovementType.ProductionConsumption, StockMovementType.AdjustmentDecrease, StockMovementType.TransferOut, StockMovementType.PurchaseReturn }.Contains(m.MovementType) ? m.Quantity : 0,
                    UserName = m.User?.Username ?? "System"
                }).ToList();

                return new PaginatedResult<StockMovementViewModel> { Items = viewModels, TotalCount = totalItems };
            }
        }

        public async Task<StockMovementFilters> GetStockMovementFiltersAsync()
        {
            using (var db = new DatabaseContext())
            {
                var types = new List<object> { "الكل" };
                types.AddRange(Enum.GetValues(typeof(StockMovementType)).Cast<object>());

                var products = await db.Products.OrderBy(p => p.Name).ToListAsync();
                var warehouses = await db.Warehouses.Where(w => w.IsActive).ToListAsync();

                return new StockMovementFilters
                {
                    MovementTypes = types,
                    Products = products,
                    Warehouses = warehouses
                };
            }
        }
    }
}