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
    public class StockTransferService : IStockTransferService
    {
        public async Task<StockTransferInitialDataDto> GetInitialDataAsync()
        {
            using (var db = new DatabaseContext())
            {
                var warehouses = await db.Warehouses.Where(w => w.IsActive).ToListAsync();
                return new StockTransferInitialDataDto { Warehouses = warehouses };
            }
        }

        public async Task<List<StorageLocation>> GetLocationsForWarehouseAsync(int warehouseId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.StorageLocations.Where(l => l.WarehouseId == warehouseId && l.IsActive).ToListAsync();
            }
        }

        public async Task<(Product product, int availableQty)> FindProductForTransferAsync(string searchText, int sourceLocationId)
        {
            using (var db = new DatabaseContext())
            {
                var product = await db.Products.FirstOrDefaultAsync(p => p.ProductCode.ToLower() == searchText.ToLower() || p.Name.ToLower().Contains(searchText.ToLower()));
                if (product == null) return (null, 0);

                var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == product.Id && i.StorageLocationId == sourceLocationId);
                return (product, inventory?.Quantity ?? 0);
            }
        }

        public async Task ExecuteTransferAsync(int sourceLocationId, int destinationLocationId, string notes, IEnumerable<StockTransferItemViewModel> items)
        {
            using (var db = new DatabaseContext())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    if (CurrentUserService.LoggedInUser == null)
                    {
                        throw new InvalidOperationException("لا يمكن إتمام العملية. المستخدم الحالي غير معروف.");
                    }
                    int currentUserId = CurrentUserService.LoggedInUser.Id;

                    var transfer = new StockTransfer
                    {
                        TransferNumber = $"TRN-{DateTime.Now:yyyyMMddHHmmss}",
                        TransferDate = DateTime.Now,
                        SourceStorageLocationId = sourceLocationId,
                        DestinationStorageLocationId = destinationLocationId,
                        Status = StockTransferStatus.Completed,
                        UserId = currentUserId,
                        Notes = notes ?? string.Empty
                    };

                    foreach (var itemVM in items)
                    {
                        transfer.StockTransferItems.Add(new StockTransferItem
                        {
                            ProductId = itemVM.ProductId,
                            Quantity = itemVM.QuantityToTransfer
                        });

                        var sourceInventory = await db.Inventories.FirstAsync(i => i.ProductId == itemVM.ProductId && i.StorageLocationId == sourceLocationId);
                        sourceInventory.Quantity -= itemVM.QuantityToTransfer;

                        var destInventory = await db.Inventories.FirstOrDefaultAsync(i => i.ProductId == itemVM.ProductId && i.StorageLocationId == destinationLocationId);
                        if (destInventory != null)
                        {
                            destInventory.Quantity += itemVM.QuantityToTransfer;
                        }
                        else
                        {
                            db.Inventories.Add(new Inventory
                            {
                                ProductId = itemVM.ProductId,
                                StorageLocationId = destinationLocationId,
                                Quantity = itemVM.QuantityToTransfer
                            });
                        }

                        var product = await db.Products.FindAsync(itemVM.ProductId);

                        db.StockMovements.Add(new StockMovement { ProductId = itemVM.ProductId, StorageLocationId = sourceLocationId, MovementDate = transfer.TransferDate, MovementType = StockMovementType.TransferOut, Quantity = itemVM.QuantityToTransfer, UnitCost = product.AverageCost, ReferenceDocument = transfer.TransferNumber, UserId = currentUserId });
                        db.StockMovements.Add(new StockMovement { ProductId = itemVM.ProductId, StorageLocationId = destinationLocationId, MovementDate = transfer.TransferDate, MovementType = StockMovementType.TransferIn, Quantity = itemVM.QuantityToTransfer, UnitCost = product.AverageCost, ReferenceDocument = transfer.TransferNumber, UserId = currentUserId });
                    }

                    db.StockTransfers.Add(transfer);
                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<List<StockTransfer>> GetTransfersHistoryAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.StockTransfers
                               .Include(t => t.SourceStorageLocation.Warehouse)
                               .Include(t => t.DestinationStorageLocation.Warehouse)
                               .Include(t => t.User)
                               .OrderByDescending(t => t.TransferDate)
                               .ToListAsync();
            }
        }
    }
}