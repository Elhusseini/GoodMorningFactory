// GoodMorningFactory/UI/ViewModels/InventoryDetailViewModel.cs
namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// يمثل تفاصيل رصيد منتج معين في موقع تخزين واحد.
    /// </summary>
    public class InventoryDetailViewModel
    {
        public string WarehouseName { get; set; }
        public string StorageLocationName { get; set; }
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int QuantityAvailable => QuantityOnHand - QuantityReserved;
    }
}