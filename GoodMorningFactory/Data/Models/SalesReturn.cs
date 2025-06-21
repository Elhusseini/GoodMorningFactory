// Data/Models/SalesReturn.cs
// *** الكود الكامل لنموذج مرتجعات المبيعات الرئيسي ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class SalesReturn
    {
        public SalesReturn()
        {
            SalesReturnItems = new HashSet<SalesReturnItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReturnNumber { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        [Required]
        public int SaleId { get; set; } // الفاتورة الأصلية
        public virtual Sale Sale { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalReturnValue { get; set; } // إجمالي قيمة المنتجات المرتجعة

        public string? Notes { get; set; }

        public virtual ICollection<SalesReturnItem> SalesReturnItems { get; set; }
    }
}