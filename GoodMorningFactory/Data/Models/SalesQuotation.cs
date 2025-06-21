// Data/Models/SalesQuotation.cs
// *** تحديث: تمت إضافة حالة جديدة لعرض السعر ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    // *** بداية التحديث ***
    public enum QuotationStatus { Draft, Sent, Accepted, Rejected, Expired, Closed }
    // *** نهاية التحديث ***

    public class SalesQuotation
    {
        public SalesQuotation()
        {
            SalesQuotationItems = new HashSet<SalesQuotationItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string QuotationNumber { get; set; }

        [Required]
        public DateTime QuotationDate { get; set; }

        [Required]
        public DateTime ValidUntilDate { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public QuotationStatus Status { get; set; }

        public string? Notes { get; set; }

        public virtual ICollection<SalesQuotationItem> SalesQuotationItems { get; set; }
    }
}