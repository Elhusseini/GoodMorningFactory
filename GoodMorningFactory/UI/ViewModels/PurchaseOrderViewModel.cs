// UI/ViewModels/PurchaseOrderViewModel.cs
// *** تحديث: تم تعديل ViewModel ليعرض العملة بشكل ديناميكي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class PurchaseOrderViewModel
    {
        public PurchaseOrder Order { get; set; }

        public string PurchaseOrderNumber => Order.PurchaseOrderNumber;
        public DateTime OrderDate => Order.OrderDate;
        public string SupplierName => Order.Supplier?.Name;
        public decimal TotalAmount => Order.TotalAmount;

        // --- بداية التحديث ---
        public string TotalAmountFormatted => $"{TotalAmount:N2} {AppSettings.DefaultCurrencySymbol}";
        // --- نهاية التحديث ---

        public DateTime? ExpectedDeliveryDate => Order.ExpectedDeliveryDate;

        // الحالات المنفصلة
        public PurchaseOrderStatus Status => Order.Status;
        public ReceiptStatus ReceiptStatus => Order.ReceiptStatus;
        public POInvoicingStatus InvoicingStatus => Order.InvoicingStatus;
    }
}
