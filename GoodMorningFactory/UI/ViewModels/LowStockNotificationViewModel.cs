// UI/ViewModels/LowStockNotificationViewModel.cs
// *** ملف جديد: ViewModel لعرض بيانات المنتجات التي وصلت لحد إعادة الطلب ***

namespace GoodMorningFactory.UI.ViewModels
{
    public class LowStockNotificationViewModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string WarehouseName { get; set; }
        public int QuantityOnHand { get; set; }
        public int ReorderLevel { get; set; }
        public string DefaultSupplierName { get; set; }
    }
}
