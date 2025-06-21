// GoodMorningFactory/UI/ViewModels/InventoryLocationDetailViewModel.cs
namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel يمثل تفاصيل رصيد منتج في موقع تخزين معين.
    /// </summary>
    public class InventoryLocationDetailViewModel
    {
        // معرفات فريدة
        public int ProductId { get; set; }
        public int StorageLocationId { get; set; }

        // تفاصيل الموقع
        public string WarehouseName { get; set; }
        public string StorageLocationName { get; set; }

        // الكميات
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int QuantityAvailable => QuantityOnHand - QuantityReserved;

        // مستويات المخزون
        public int ReorderLevel { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }
    }
}