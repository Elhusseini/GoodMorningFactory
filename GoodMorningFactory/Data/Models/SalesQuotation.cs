// Data/Models/SalesQuotation.cs
// *** تحديث: تمت إضافة حقول لحساب الضريبة ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum QuotationStatus { Draft, Sent, Accepted, Rejected, Expired, Closed }

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

        // --- بداية التحديث: إضافة حقول الضريبة ---
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Subtotal { get; set; } // الإجمالي قبل الضريبة

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TaxAmount { get; set; } // مبلغ الضريبة

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; } // الإجمالي النهائي بعد الضريبة
        // --- نهاية التحديث ---

        [Required]
        public QuotationStatus Status { get; set; }

        public string? Notes { get; set; }

        public virtual ICollection<SalesQuotationItem> SalesQuotationItems { get; set; }
    }
}
