// UI/ViewModels/InventoryViewModel.cs
// *** تحديث شامل: تم تعديل النموذج ليعرض المخزون على مستوى الموقع الفرعي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public enum StockStatus
    {
        [Description("متوفر")]
        Available,
        [Description("كمية منخفضة")]
        LowStock,
        [Description("نفد المخزون")]
        OutOfStock
    }

    public class InventoryViewModel
    {
        public int ProductId { get; set; }
        public int StorageLocationId { get; set; } // ** إضافة جديدة **
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string WarehouseName { get; set; }
        public string StorageLocationName { get; set; } // ** إضافة جديدة **

        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int QuantityAvailable => QuantityOnHand - QuantityReserved;

        public int ReorderLevel { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }

        public decimal AverageCost { get; set; }
        public decimal TotalStockValue => QuantityOnHand * AverageCost;

        public string AverageCostFormatted => $"{AverageCost:N2} {AppSettings.DefaultCurrencySymbol}";
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
