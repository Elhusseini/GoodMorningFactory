// UI/ViewModels/PurchaseViewModel.cs
// *** تحديث: تمت إضافة خصائص منسقة لعرض العملة بشكل ديناميكي ***
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.Core.Services;
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class PurchaseViewModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public Supplier Supplier { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public PurchaseInvoiceStatus Status { get; set; }

        // --- بداية التحديث: إضافة خصائص منسقة للعملة ---
        public string TotalAmountFormatted => $"{TotalAmount:N2} {AppSettings.DefaultCurrencySymbol}";
        public string AmountPaidFormatted => $"{AmountPaid:N2} {AppSettings.DefaultCurrencySymbol}";
        public string BalanceDueFormatted => $"{BalanceDue:N2} {AppSettings.DefaultCurrencySymbol}";
        // --- نهاية التحديث ---

        public decimal BalanceDue => TotalAmount - AmountPaid;
    }
}
