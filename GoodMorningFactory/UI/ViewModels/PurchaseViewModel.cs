// UI/ViewModels/PurchaseViewModel.cs
// *** ملف جديد: ViewModel خاص بعرض فواتير الموردين مع الرصيد المستحق ***
using GoodMorningFactory.Data.Models;
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    // هذا الكلاس يرث من كلاس فاتورة الشراء الأصلي ويضيف إليه خاصية الرصيد المحسوب
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

        // خاصية محسوبة لعرض الرصيد المستحق للمورد
        public decimal BalanceDue => TotalAmount - AmountPaid;
    }
}