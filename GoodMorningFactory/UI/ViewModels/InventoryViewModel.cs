// UI/ViewModels/InventoryViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;

namespace GoodMorningFactory.UI.ViewModels
{
    public class InventoryViewModel
    {
        public int ProductId { get; set; }
        public int StorageLocationId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string WarehouseName { get; set; }
        public string StorageLocationName { get; set; }
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int QuantityAvailable => QuantityOnHand - QuantityReserved;
        public int ReorderLevel { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }

        // --- بداية التعديل ---
        public decimal AverageCost { get; set; }
        public decimal LastPurchasePrice { get; set; } // خاصية جديدة لآخر سعر شراء

        // إعادة الحساب ليعتمد على متوسط التكلفة
        public decimal TotalStockValue => QuantityOnHand * AverageCost;

        // خصائص منسقة جديدة
        public string AverageCostFormatted => $"{AverageCost:N2} {AppSettings.DefaultCurrencySymbol}";
        public string LastPurchasePriceFormatted => $"{LastPurchasePrice:N2} {AppSettings.DefaultCurrencySymbol}";
        // --- نهاية التعديل ---

        public string TotalStockValueFormatted => $"{TotalStockValue:N2} {AppSettings.DefaultCurrencySymbol}";

        public StockStatus Status
        {
            get
            {
                if (QuantityAvailable <= 0) return StockStatus.OutOfStock;
                if (QuantityAvailable <= ReorderLevel && ReorderLevel > 0) return StockStatus.LowStock;
                return StockStatus.Available;
            }
        }
    }
}