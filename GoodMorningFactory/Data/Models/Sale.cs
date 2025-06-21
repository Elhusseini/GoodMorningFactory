// Data/Models/Sale.cs
// *** تحديث: تمت إضافة حالة للفاتورة (InvoiceStatus) ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    // *** بداية التحديث: تعريف حالات الفاتورة ***
    public enum InvoiceStatus { Draft, Sent, PartiallyPaid, Paid, Overdue, Cancelled }
    // *** نهاية التحديث ***

    public class Sale
    {
        public Sale()
        {
            SaleItems = new HashSet<SaleItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string InvoiceNumber { get; set; }

        [Required]
        public DateTime SaleDate { get; set; }

        // *** بداية التحديث: إضافة حقل الحالة ***
        [Required]
        public InvoiceStatus Status { get; set; }
        // *** نهاية التحديث ***

        public int? SalesOrderId { get; set; }
        public virtual SalesOrder SalesOrder { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountPaid { get; set; }

        public virtual ICollection<SaleItem> SaleItems { get; set; }
    }
}