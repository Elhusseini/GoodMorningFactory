// UI/ViewModels/InventoryViewModel.cs
// *** تحديث: تمت إضافة حقول جديدة لعرض حالة المخزون بشكل تفصيلي ***
namespace GoodMorningFactory.UI.ViewModels
{
    public enum StockStatus { Available, LowStock, OutOfStock }

    public class InventoryViewModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string WarehouseName { get; set; } // <-- إضافة جديدة
        public int Quantity { get; set; }
        public int QuantityOnHand { get; set; } // الكمية الحالية في المخزن
        public int QuantityReserved { get; set; } // الكمية المحجوزة لأوامر البيع
        public int QuantityAvailable => QuantityOnHand - QuantityReserved; // الكمية المتاحة فعلياً
        public int ReorderLevel { get; set; } // حد إعادة الطلب

        // خاصية محسوبة لتحديد حالة المخزون
        public StockStatus Status
        {
            get
            {
                if (QuantityAvailable <= 0) return StockStatus.OutOfStock;
                if (QuantityAvailable <= ReorderLevel) return StockStatus.LowStock;
                return StockStatus.Available;
            }
        }
    }
}