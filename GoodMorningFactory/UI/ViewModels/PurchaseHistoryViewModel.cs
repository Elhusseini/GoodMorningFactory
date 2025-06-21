using GoodMorningFactory.Core.Services;
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// يمثل سطراً واحداً في سجل أسعار الشراء.
    /// </summary>
    public class PurchaseHistoryViewModel
    {
        public DateTime PurchaseDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string SupplierName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string UnitPriceFormatted => $"{UnitPrice:N2} {AppSettings.DefaultCurrencySymbol}";
    }
}