// UI/ViewModels/AccountsReceivableViewModel.cs
// *** تحديث: تم تعديل الـ ViewModel لعرض تفاصيل الفواتير وأعمار الديون ***
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AccountsReceivableViewModel
    {
        public int SaleId { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal BalanceDue { get; set; }

        // خصائص أعمار الديون
        public decimal Bucket0_30 { get; set; } // مستحق خلال 30 يوم
        public decimal Bucket31_60 { get; set; } // مستحق خلال 31-60 يوم
        public decimal Bucket61_90 { get; set; } // مستحق خلال 61-90 يوم
        public decimal BucketOver90 { get; set; } // متأخر لأكثر من 90 يوم
    }
}