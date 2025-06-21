using System;
using System.Collections.Generic;
using System.ComponentModel; // <-- إضافة مهمة لدعم [Description]
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    /// <summary>
    /// يمثل حالات دورة حياة فاتورة المبيعات.
    /// تمت إضافة [Description] لعرض أسماء عربية واضحة في الواجهة.
    /// </summary>
    public enum InvoiceStatus
    {
        [Description("مسودة")]
        Draft,
        [Description("مرسلة")]
        Sent,
        [Description("مدفوعة جزئياً")]
        PartiallyPaid,
        [Description("مدفوعة")]
        Paid,
        [Description("متأخرة")]
        Overdue,
        [Description("ملغاة")]
        Cancelled
    }

    /// <summary>
    /// يمثل فاتورة مبيعات في النظام.
    /// </summary>
    public class Sale
    {
        public Sale()
        {
            SaleItems = new HashSet<SaleItem>();
            // هذا السطر ضروري لإنشاء العلاقة بين الفاتورة ومرتجعاتها
            SalesReturns = new HashSet<SalesReturn>();
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
        // --- بداية الإضافة ---
        // هذه هي الخاصية التي كانت ناقصة وتسببت في الخطأ
        public virtual ICollection<SalesReturn> SalesReturns { get; set; }
        // --- نهاية الإضافة ---
    }
}
