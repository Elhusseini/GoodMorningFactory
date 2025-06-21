using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel يمثل أمر بيع واحد للعرض في الجدول.
    /// </summary>
    public class SalesOrderViewModel : BaseViewModel
    {
        public SalesOrder Order { get; set; }

        public int Id => Order.Id;
        public string SalesOrderNumber => Order.SalesOrderNumber;
        public DateTime OrderDate => Order.OrderDate;
        public string CustomerName => Order.Customer?.CustomerName;
        public decimal TotalAmount => Order.TotalAmount;
        public DateTime? ExpectedShipDate => Order.ExpectedShipDate;
        public string TotalAmountFormatted => $"{TotalAmount:N2} {AppSettings.DefaultCurrencySymbol}";
        public OrderStatus OrderStatus => Order.Status;
        public ShippingStatus ShippingStatus => Order.ShippingStatus;
        public InvoicingStatus InvoicingStatus => Order.InvoicingStatus;
    }
}
