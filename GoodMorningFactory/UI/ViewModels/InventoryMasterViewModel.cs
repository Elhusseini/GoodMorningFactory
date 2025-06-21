// GoodMorningFactory/UI/ViewModels/InventoryMasterViewModel.cs
using GoodMorningFactory.Core.Services;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// يمثل منتجاً واحداً في القائمة الرئيسية المجمّعة للمخزون.
    /// </summary>
    public class InventoryMasterViewModel : BaseViewModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int TotalQuantityOnHand { get; set; }
        public int TotalQuantityReserved { get; set; }
        public int TotalQuantityAvailable => TotalQuantityOnHand - TotalQuantityReserved;
        public decimal AverageCost { get; set; }
        public decimal TotalValue => TotalQuantityOnHand * AverageCost;
        public string TotalValueFormatted => $"{TotalValue:N2} {AppSettings.DefaultCurrencySymbol}";
    }
}