// GoodMorningFactory/Core/Services/ProductService.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GoodMorningFactory.Core.Services
{
    public class ProductService : IProductService
    {
        public async Task<PaginatedResult<ProductViewModel>> GetProductsAsync(ProductFilterCriteria criteria)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.Products.Include(p => p.Category).AsQueryable();

                if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                    query = query.Where(p => p.Name.ToLower().Contains(criteria.SearchText.ToLower()) || p.ProductCode.ToLower().Contains(criteria.SearchText.ToLower()));
                if (criteria.CategoryId != 0)
                    query = query.Where(p => p.CategoryId == criteria.CategoryId);
                if (criteria.SupplierId != 0)
                    query = query.Where(p => p.DefaultSupplierId == criteria.SupplierId);
                if (criteria.ProductType.HasValue)
                    query = query.Where(p => p.ProductType == criteria.ProductType.Value);
                if (criteria.IsActive.HasValue)
                    query = query.Where(p => p.IsActive == criteria.IsActive.Value);

                int totalItems = await query.CountAsync();

                var productsForPage = await query.OrderBy(p => p.Name)
                                .Skip((criteria.Page - 1) * criteria.PageSize)
                                .Take(criteria.PageSize)
                                .ToListAsync();

                var productIds = productsForPage.Select(p => p.Id).ToList();
                var inventories = await db.Inventories
                             .Where(i => productIds.Contains(i.ProductId))
                             .GroupBy(i => i.ProductId)
                             .Select(g => new { ProductId = g.Key, TotalStock = g.Sum(i => i.Quantity) })
                             .ToDictionaryAsync(i => i.ProductId, i => i.TotalStock);

                var productViewModels = productsForPage.Select(p =>
                {
                    BitmapImage image = null;
                    if (p.ProductImage != null && p.ProductImage.Length > 0)
                    {
                        image = new BitmapImage();
                        using (var stream = new MemoryStream(p.ProductImage))
                        {
                            image.BeginInit();
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.DecodePixelWidth = 60;
                            image.StreamSource = stream;
                            image.EndInit();
                        }
                        image.Freeze();
                    }

                    return new ProductViewModel
                    {
                        Id = p.Id,
                        ProductCode = p.ProductCode,
                        Name = p.Name,
                        CategoryName = p.Category?.Name,
                        ProductType = p.ProductType,
                        PurchasePrice = p.PurchasePrice,
                        SalePrice = p.SalePrice,
                        CurrentStock = inventories.ContainsKey(p.Id) ? inventories[p.Id] : 0,
                        IsActive = p.IsActive,
                        ProductImage = image
                    };
                }).ToList();

                return new PaginatedResult<ProductViewModel>
                {
                    Items = productViewModels,
                    TotalCount = totalItems
                };
            }
        }

        public async Task<AddEditProductDto> GetInitialDataForAddEditWindowAsync(int? productId, int? sourceProductIdToCopy)
        {
            using (var db = new DatabaseContext())
            {
                var dto = new AddEditProductDto
                {
                    Categories = await db.Categories.OrderBy(c => c.Name).ToListAsync(),
                    UnitsOfMeasure = await db.UnitsOfMeasure.OrderBy(u => u.Name).ToListAsync(),
                    Suppliers = await db.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(),
                    Currencies = await db.Currencies.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(),
                    TaxRules = await db.TaxRules.ToListAsync(),
                    StorageLocations = await db.StorageLocations.Where(sl => sl.IsActive).OrderBy(sl => sl.Name).ToListAsync(),
                    DefaultCurrencyId = (await db.CompanyInfos.FirstOrDefaultAsync())?.DefaultCurrencyId
                };

                int? idToLoad = productId ?? sourceProductIdToCopy;

                if (idToLoad.HasValue)
                {
                    dto.Product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == idToLoad.Value);
                    dto.Inventory = await db.Inventories.AsNoTracking().FirstOrDefaultAsync(i => i.ProductId == idToLoad.Value);
                }

                if (dto.Product == null)
                {
                    dto.Product = new Product
                    {
                        ProductCode = "اختر فئة لتوليد الكود",
                        IsActive = true,
                        CurrencyId = dto.DefaultCurrencyId ?? 1
                    };
                }
                return dto;
            }
        }

        public async Task<int> SaveProductAsync(Product product, bool trackInventory, int? primaryStorageLocationId, int reorderLevel, int minStock, int maxStock)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    if (product.Id == 0)
                    {
                        if (await db.Products.AnyAsync(p => p.ProductCode == product.ProductCode))
                        {
                            throw new Exception("كود المنتج مستخدم بالفعل. يرجى اختيار كود آخر.");
                        }
                        db.Products.Add(product);
                    }
                    else
                    {
                        db.Products.Update(product);
                    }
                    await db.SaveChangesAsync();

                    var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == product.Id);
                    if (trackInventory)
                    {
                        if (inventory == null)
                        {
                            if (!primaryStorageLocationId.HasValue)
                                throw new Exception("يجب تحديد موقع تخزين أساسي عند تفعيل تتبع المخزون لمنتج جديد.");

                            inventory = new Inventory
                            {
                                ProductId = product.Id,
                                StorageLocationId = primaryStorageLocationId.Value,
                                Quantity = 0
                            };
                            db.Inventories.Add(inventory);
                        }
                        inventory.ReorderLevel = reorderLevel;
                        inventory.MinStockLevel = minStock;
                        inventory.MaxStockLevel = maxStock;
                    }
                    else
                    {
                        if (inventory != null) db.Inventories.Remove(inventory);
                    }

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return product.Id;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task DeleteProductAsync(int productId)
        {
            using (var db = new DatabaseContext())
            {
                var totalStock = await db.Inventories
                            .Where(i => i.ProductId == productId)
                            .SumAsync(i => i.Quantity);

                if (totalStock > 0)
                {
                    throw new InvalidOperationException($"لا يمكن حذف المنتج لوجود رصيد حالي له في المخزن ({totalStock} وحدة).");
                }

                bool isLinkedToTransactions = await db.SaleItems.AnyAsync(i => i.ProductId == productId) ||
                               await db.PurchaseItems.AnyAsync(i => i.ProductId == productId) ||
                               await db.PurchaseOrderItems.AnyAsync(i => i.ProductId == productId) ||
                               await db.SalesOrderItems.AnyAsync(i => i.ProductId == productId);

                if (isLinkedToTransactions)
                {
                    throw new InvalidOperationException("لا يمكن حذف المنتج لارتباطه بفواتير أو أوامر شراء أو عمليات سابقة.");
                }

                var productToDelete = await db.Products.FindAsync(productId);
                if (productToDelete != null)
                {
                    db.Products.Remove(productToDelete);
                    await db.SaveChangesAsync();
                }
            }
        }
        public async Task<List<Product>> SearchProductsToCopyAsync(string searchText)
        {
            using (var db = new DatabaseContext())
            {
                if (string.IsNullOrEmpty(searchText))
                {
                    return await db.Products.OrderBy(p => p.Name).Take(50).ToListAsync();
                }
                else
                {
                    return await db.Products
                      .Where(p => p.Name.ToLower().Contains(searchText.ToLower()) || p.ProductCode.ToLower().Contains(searchText.ToLower()))
                      .OrderBy(p => p.Name)
                      .ToListAsync();
                }
            }
        }

        public async Task<string> GenerateNextProductCodeAsync(int categoryId)
        {
            using (var db = new DatabaseContext())
            {
                var category = await db.Categories.FindAsync(categoryId);
                if (category?.CategoryCode == null)
                {
                    return "كود فئة غير صالح";
                }

                string prefix = $"{category.CategoryCode}-";
                var lastProductCode = await db.Products
                  .Where(p => p.ProductCode.StartsWith(prefix))
                  .OrderByDescending(p => p.ProductCode)
                  .Select(p => p.ProductCode)
                  .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastProductCode != null)
                {
                    string numberPart = lastProductCode.Substring(prefix.Length);
                    int.TryParse(numberPart, out int lastNumber);
                    nextNumber = lastNumber + 1;
                }
                return $"{prefix}{nextNumber:D5}";
            }
        }
    }
}