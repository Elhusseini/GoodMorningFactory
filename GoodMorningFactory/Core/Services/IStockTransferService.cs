using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IStockTransferService
    {
        Task<StockTransferInitialDataDto> GetInitialDataAsync();
        Task<List<StorageLocation>> GetLocationsForWarehouseAsync(int warehouseId);
        Task<(Product product, int availableQty)> FindProductForTransferAsync(string searchText, int sourceLocationId);
        Task ExecuteTransferAsync(int sourceLocationId, int destinationLocationId, string notes, IEnumerable<StockTransferItemViewModel> items);
        Task<List<StockTransfer>> GetTransfersHistoryAsync();
    }

    public class StockTransferInitialDataDto
    {
        public List<Warehouse> Warehouses { get; set; }
    }
}