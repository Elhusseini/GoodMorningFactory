// GoodMorningFactory/Core/Services/IWarehouseService.cs
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف العقد الخاص بخدمات المخازن والمواقع الفرعية.
    /// </summary>
    public interface IWarehouseService
    {
        // --- دوال المخازن الرئيسية ---
        Task<List<Warehouse>> GetWarehousesAsync();
        Task<Warehouse> GetWarehouseByIdAsync(int warehouseId);
        Task SaveWarehouseAsync(Warehouse warehouse);

        // --- دوال المواقع الفرعية ---
        Task<List<StorageLocation>> GetLocationsForWarehouseAsync(int warehouseId);
        Task<StorageLocation> GetLocationByIdAsync(int locationId);
        Task SaveLocationAsync(StorageLocation location);
        Task DeleteLocationAsync(int locationId);
    }
}