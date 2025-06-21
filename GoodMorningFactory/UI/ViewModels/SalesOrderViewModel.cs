// UI/ViewModels/SalesOrderViewModel.cs
// *** تحديث: تمت إضافة خاصية منسقة للعملة ***
using GoodMorningFactory.Core.Services; // <-- إضافة مهمة
using GoodMorningFactory.Data.Models;
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class SalesOrderViewModel
    {
        public SalesOrder Order { get; set; }

        public string SalesOrderNumber => Order.SalesOrderNumber;
        public DateTime OrderDate => Order.OrderDate;
        public string CustomerName => Order.Customer?.CustomerName;
        public decimal TotalAmount => Order.TotalAmount;
        public DateTime? ExpectedShipDate => Order.ExpectedShipDate;

        // --- بداية الإضافة ---
        public string TotalAmountFormatted => $"{TotalAmount:N2} {AppSettings.DefaultCurrencySymbol}";
        // --- نهاية الإضافة ---

        public OrderStatus OrderStatus => Order.Status;
        public ShippingStatus ShippingStatus => Order.ShippingStatus;
        public InvoicingStatus InvoicingStatus => Order.InvoicingStatus;
    }
}
