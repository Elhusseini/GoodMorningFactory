// GoodMorningFactory/Core/Services/IInventoryService.cs
// *** الكود الكامل والمعدل ***
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IInventoryService
    {
        // --- الدوال الموجودة مسبقاً (تبقى كما هي) ---
        Task<PaginatedResult<GroupedInventoryViewModel>> GetInventoryStatusAsync(InventoryFilterCriteria criteria);
        Task<InventoryFilters> GetInventoryFiltersAsync();
        Task<QuickAdjustDataDto> GetDataForQuickAdjustmentAsync(int productId, int storageLocationId);
        Task PerformQuickAdjustmentAsync(int productId, int storageLocationId, int newQuantity, string reason);
        Task<List<ProductStockLocationViewModel>> GetStockLevelsForProductAsync(int productId);
        Task UpdateStockLevelsForProductAsync(int productId, IEnumerable<ProductStockLocationViewModel> stockLevels, string reason);

        // ======================= بداية الإضافة =======================

        /// <summary>
        /// جلب قائمة بالمخازن النشطة.
        /// </summary>
        Task<List<Warehouse>> GetActiveWarehousesAsync();

        /// <summary>
        /// جلب قائمة بالمواقع الفرعية النشطة لمخزن معين.
        /// </summary>
        Task<List<StorageLocation>> GetActiveLocationsByWarehouseAsync(int warehouseId);

        /// <summary>
        /// البحث عن منتج وإرجاع بياناته اللازمة لعملية الجرد.
        /// </summary>
        Task<StockAdjustmentItemViewModel> GetProductForAdjustmentAsync(string searchText, int locationId);

        /// <summary>
        /// ترحيل عملية تسوية المخزون الكاملة (إنشاء سجل التسوية، تحديث المخزون، إنشاء حركة مخزنية، إنشاء قيد يومية).
        /// </summary>
        Task PostStockAdjustmentAsync(StockAdjustment adjustment);

        // ======================== نهاية الإضافة ========================

        // ======================= بداية الإضافة =======================
        /// <summary>
        /// جلب قائمة بالمنتجات التي وصلت لحد إعادة الطلب أو أقل.
        /// </summary>
        Task<List<LowStockNotificationViewModel>> GetLowStockItemsAsync();
        // ======================== نهاية الإضافة ========================

        // ======================= بداية الإضافة =======================
        /// <summary>
        /// جلب سجل حركات المخزون لمنتج معين.
        /// </summary>
        Task<List<StockMovementViewModel>> GetStockMovementsForProductAsync(int productId);
        // ======================== نهاية الإضافة ========================

        // ======================= بداية الإضافة =======================
        /// <summary>
        /// جلب قائمة بالأرقام التسلسلية مع دعم للبحث.
        /// </summary>
        Task<List<SerialNumber>> GetSerialNumbersAsync(string filter);
        // ======================== نهاية الإضافة ========================
    }


    // --- الكلاسات المساعدة (تبقى كما هي) ---
    public class InventoryFilterCriteria
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SearchText { get; set; }
        public int? WarehouseId { get; set; }
        public int? CategoryId { get; set; }
        public StockStatus? Status { get; set; }
    }

    public class InventoryFilters
    {
        public List<Warehouse> Warehouses { get; set; }
        public List<Category> Categories { get; set; }
        public List<object> Statuses { get; set; }
    }

    public class QuickAdjustDataDto
    {
        public string ProductName { get; set; }
        public string StorageLocationName { get; set; }
        public int SystemQuantity { get; set; }
    }
}