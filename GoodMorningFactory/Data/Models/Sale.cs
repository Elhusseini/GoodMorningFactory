// Data/Models/Sale.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum InvoiceStatus { Draft, Sent, PartiallyPaid, Paid, Overdue, Cancelled }

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

        public DateTime? DueDate { get; set; }

        [Required]
        public InvoiceStatus Status { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public int? SalesOrderId { get; set; }
        public virtual SalesOrder SalesOrder { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Subtotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TaxAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountPaid { get; set; }

        public virtual ICollection<SaleItem> SaleItems { get; set; }
    }
}
