// Data/Models/SalesOrder.cs
// *** تحديث: تمت إضافة حالة "ملغي" لأمر البيع ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    // تم تحديث enum ليشمل جميع الحالات
    public enum OrderStatus { New, Confirmed, InProcess, PartiallyShipped, Shipped, Invoiced, Cancelled }

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
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        public virtual ICollection<SalesOrderItem> SalesOrderItems { get; set; }
    }
}