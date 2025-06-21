// UI/ViewModels/SalesViewViewModel.cs
// *** تحديث: تمت إضافة خصائص منسقة لعرض العملة بشكل صحيح ***
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.Core.Services;
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    // ViewModel لعرض الفواتير في الجدول
    public class SalesViewViewModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public SalesOrder SalesOrder { get; set; }
        public string CustomerName { get; set; }
        public DateTime SaleDate { get; set; }
        public DateTime? DueDate { get; set; }
        public InvoiceStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal BalanceDue => TotalAmount - AmountPaid;

        // --- بداية التحديث: إضافة خصائص منسقة للعملة ---
        public string TotalAmountFormatted => $"{TotalAmount:N2} {AppSettings.DefaultCurrencySymbol}";
        public string AmountPaidFormatted => $"{AmountPaid:N2} {AppSettings.DefaultCurrencySymbol}";
        public string BalanceDueFormatted => $"{BalanceDue:N2} {AppSettings.DefaultCurrencySymbol}";
        // --- نهاية التحديث ---
    }
}
