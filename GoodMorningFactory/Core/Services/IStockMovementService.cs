using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IStockMovementService
    {
        Task<PaginatedResult<StockMovementViewModel>> GetStockMovementsAsync(StockMovementFilterCriteria criteria);
        Task<StockMovementFilters> GetStockMovementFiltersAsync();
    }

    public class StockMovementFilterCriteria
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public string SearchText { get; set; }
        public StockMovementType? MovementType { get; set; }
        public int? ProductId { get; set; }
        public int? WarehouseId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class StockMovementFilters
    {
        public List<object> MovementTypes { get; set; }
        public List<Product> Products { get; set; }
        public List<Warehouse> Warehouses { get; set; }
    }
}