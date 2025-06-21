// Data/Models/SalesOrder.cs
// *** تحديث: تمت إضافة وصف عربي للحالات لتحسين العرض في الواجهة ***
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum OrderStatus
    {
        [Description("جديد")]
        New,
        [Description("مؤكد")]
        Confirmed,
        [Description("قيد التنفيذ")]
        InProcess,
        [Description("مشحون جزئياً")]
        PartiallyShipped,
        [Description("مشحون")]
        Shipped,
        [Description("مفوتر")]
        Invoiced,
        [Description("ملغي")]
        Cancelled
    }

    public enum ShippingStatus
    {
        [Description("لم يتم الشحن")]
        NotShipped,
        [Description("مشحون جزئياً")]
        PartiallyShipped,
        [Description("مشحون بالكامل")]
        FullyShipped
    }

    public enum InvoicingStatus
    {
        [Description("غير مفوتر")]
        NotInvoiced,
        [Description("مفوتر جزئياً")]
        PartiallyInvoiced,
        [Description("مفوتر بالكامل")]
        FullyInvoiced
    }

    public class SalesOrder
    {
        public SalesOrder()
        {
            SalesOrderItems = new HashSet<SalesOrderItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string SalesOrderNumber { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime? ExpectedShipDate { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public int? SalesQuotationId { get; set; }
        public virtual SalesQuotation SalesQuotation { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        [Required]
        public ShippingStatus ShippingStatus { get; set; } = ShippingStatus.NotShipped;

        [Required]
        public InvoicingStatus InvoicingStatus { get; set; } = InvoicingStatus.NotInvoiced;

        public virtual ICollection<SalesOrderItem> SalesOrderItems { get; set; }
    }
}
