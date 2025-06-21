// UI/ViewModels/AccountsPayableViewModel.cs
// *** ملف جديد: ViewModel خاص بعرض فواتير الموردين وأعمار ديونها ***
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AccountsPayableViewModel
    {
        public int PurchaseId { get; set; }
        public string SupplierName { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal BalanceDue { get; set; }

        // خصائص أعمار الديون
        public decimal Bucket0_30 { get; set; }
        public decimal Bucket31_60 { get; set; }
        public decimal Bucket61_90 { get; set; }
        public decimal BucketOver90 { get; set; }
    }
}