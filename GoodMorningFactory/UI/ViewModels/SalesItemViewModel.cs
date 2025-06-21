// GoodMorningFactory/UI/ViewModels/SalesItemViewModel.cs
using GoodMorningFactory.Core.Helpers; // لاستخدام GetDescription()
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// يمثل هذا الكلاس فاتورة بيع واحدة لعرضها في الواجهة.
    /// </summary>
    public class SalesItemViewModel : BaseViewModel
    {
        // --- بداية الإضافة: إضافة مُنشئ (Constructor) يقبل بيانات الفاتورة ---
        /// <summary>
        /// مُنشئ فارغ لوقت التصميم.
        /// </summary>
        public SalesItemViewModel() { }

        /// <summary>
        /// المُنشئ الرئيسي الذي يقوم بتعبئة بيانات الـ ViewModel من كائن بيانات الخدمة.
        /// </summary>
        /// <param name="summaryDto">كائن البيانات القادم من الخدمة.</param>
        public SalesItemViewModel(SaleSummaryDto summaryDto)
        {
            Id = summaryDto.Sale.Id;
            InvoiceNumber = summaryDto.Sale.InvoiceNumber;
            CustomerName = summaryDto.Sale.Customer.CustomerName;
            SaleDate = summaryDto.Sale.SaleDate;
            DueDate = summaryDto.Sale.DueDate;
            Status = summaryDto.Sale.Status;
            TotalAmount = summaryDto.Sale.TotalAmount;
            AmountPaid = summaryDto.Sale.AmountPaid;
            TotalReturned = summaryDto.TotalReturned;
        }
        // --- نهاية الإضافة ---

        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime SaleDate { get; set; }
        public DateTime? DueDate { get; set; }
        public InvoiceStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TotalReturned { get; set; }

        public decimal BalanceDue => TotalAmount - TotalReturned - AmountPaid;

        public string TotalAmountFormatted => $"{TotalAmount:N2} {AppSettings.DefaultCurrencySymbol}";
        public string AmountPaidFormatted => $"{AmountPaid:N2} {AppSettings.DefaultCurrencySymbol}";
        public string BalanceDueFormatted => $"{BalanceDue:N2} {AppSettings.DefaultCurrencySymbol}";

        // --- بداية الإضافة: خاصية جديدة لعرض وصف الحالة ---
        /// <summary>
        /// تقوم بإرجاع الوصف العربي لحالة الفاتورة.
        /// </summary>
        public string StatusDescription => Status.GetDescription();
        // --- نهاية الإضافة ---
    }
}