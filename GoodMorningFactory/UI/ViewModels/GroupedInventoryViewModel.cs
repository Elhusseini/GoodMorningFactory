// GoodMorningFactory/UI/ViewModels/GroupedInventoryViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel يمثل عرضًا مجمّعًا للمنتج الواحد عبر كل مواقع التخزين.
    /// </summary>
    public class GroupedInventoryViewModel : BaseViewModel // يرث من BaseViewModel لدعم الإشعارات
    {
        // الخصائص الأساسية للمنتج
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public decimal AverageCost { get; set; }
        public decimal LastPurchasePrice { get; set; }

        /// <summary>
        /// قائمة تفصيلية بأرصدة هذا المنتج في كل موقع تخزين.
        /// </summary>
        public List<InventoryLocationDetailViewModel> LocationDetails { get; set; }

        // --- الخصائص المجمّعة التي يتم حسابها ---

        public int TotalQuantityOnHand => LocationDetails.Sum(d => d.QuantityOnHand);
        public int TotalQuantityReserved => LocationDetails.Sum(d => d.QuantityReserved);
        public int TotalQuantityAvailable => TotalQuantityOnHand - TotalQuantityReserved;
        public decimal TotalStockValue => TotalQuantityOnHand * AverageCost;

        // --- الخصائص المنسقة للعرض ---

        public string AverageCostFormatted => $"{AverageCost:N2} {AppSettings.DefaultCurrencySymbol}";
        public string LastPurchasePriceFormatted => $"{LastPurchasePrice:N2} {AppSettings.DefaultCurrencySymbol}";
        public string TotalStockValueFormatted => $"{TotalStockValue:N2} {AppSettings.DefaultCurrencySymbol}";

        /// <summary>
        /// الحالة الإجمالية للمخزون بناءً على الكمية الإجمالية المتاحة.
        /// </summary>
        public StockStatus Status
        {
            get
            {
                // نأخذ أعلى مستوى لإعادة الطلب من بين جميع المواقع كمرجع
                var reorderLevel = LocationDetails.Max(d => (int?)d.ReorderLevel) ?? 0;
                if (TotalQuantityAvailable <= 0) return StockStatus.OutOfStock;
                if (reorderLevel > 0 && TotalQuantityAvailable <= reorderLevel) return StockStatus.LowStock;
                return StockStatus.Available;
            }
        }

        public GroupedInventoryViewModel()
        {
            LocationDetails = new List<InventoryLocationDetailViewModel>();
        }
    }
}