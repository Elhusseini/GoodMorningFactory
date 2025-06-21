// UI/ViewModels/ProductViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using System.Windows.Media.Imaging;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ProductViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string ProductCode { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public ProductType ProductType { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public int CurrentStock { get; set; }
        public bool IsActive { get; set; }
        public BitmapImage ProductImage { get; set; }

        // --- بداية الإضافة: خاصية جديدة لتحديد حالة المخزون ---
        // سنفترض مؤقتًا أن مستوى إعادة الطلب هو 10 للمثال
        public StockStatus StockStatus
        {
            get
            {
                if (CurrentStock <= 0) return StockStatus.OutOfStock;
                // يمكنك ربط هذا بمستوى إعادة الطلب الحقيقي للمنتج لاحقًا
                if (CurrentStock <= 10) return StockStatus.LowStock;
                return StockStatus.Available;
            }
        }
        // --- نهاية الإضافة ---

        public string SalePriceFormatted => $"{SalePrice:N2} {AppSettings.DefaultCurrencySymbol}";
        public string PurchasePriceFormatted => $"{PurchasePrice:N2} {AppSettings.DefaultCurrencySymbol}";
    }
}