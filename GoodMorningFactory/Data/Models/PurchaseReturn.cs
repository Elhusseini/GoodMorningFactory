// Data/Models/PurchaseReturn.cs
// *** ملف جديد: يمثل جدول مرتجعات المشتريات الرئيسي ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class PurchaseReturn
    {
        public PurchaseReturn()
        {
            PurchaseReturnItems = new HashSet<PurchaseReturnItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReturnNumber { get; set; } // رقم إشعار المدين

        [Required]
        public DateTime ReturnDate { get; set; }

        [Required]
        public int PurchaseId { get; set; } // فاتورة الشراء الأصلية
        public virtual Purchase Purchase { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalReturnValue { get; set; } // إجمالي قيمة المنتجات المرتجعة

        public string? Notes { get; set; }

        public virtual ICollection<PurchaseReturnItem> PurchaseReturnItems { get; set; }
    }
}