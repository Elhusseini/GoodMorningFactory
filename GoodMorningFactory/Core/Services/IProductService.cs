// GoodMorningFactory/Core/Services/IProductService.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IProductService
    {
        Task<PaginatedResult<ProductViewModel>> GetProductsAsync(ProductFilterCriteria criteria);
        Task DeleteProductAsync(int productId);
        Task<AddEditProductDto> GetInitialDataForAddEditWindowAsync(int? productId, int? sourceProductIdToCopy);
        // --- بداية التعديل: إضافة معرّف الموقع المختار إلى دالة الحفظ ---
        Task<int> SaveProductAsync(Product product, bool trackInventory, int? primaryStorageLocationId, int reorderLevel, int minStock, int maxStock);
        // --- نهاية التعديل ---
        Task<List<Product>> SearchProductsToCopyAsync(string searchText);
        Task<string> GenerateNextProductCodeAsync(int categoryId);
    }

    public class AddEditProductDto
    {
        public Product Product { get; set; }
        public Inventory Inventory { get; set; }
        public List<Category> Categories { get; set; }
        public List<UnitOfMeasure> UnitsOfMeasure { get; set; }
        public List<Supplier> Suppliers { get; set; }
        public List<Currency> Currencies { get; set; }
        public List<TaxRule> TaxRules { get; set; }
        // --- بداية الإضافة: إضافة قائمة مواقع التخزين ---
        public List<StorageLocation> StorageLocations { get; set; }
        // --- نهاية الإضافة ---
        public int? DefaultCurrencyId { get; set; }
    }
}