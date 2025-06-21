// Data/Models/PurchaseReturn.cs
// *** الكود الكامل والمعدل ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace GoodMorningFactory.Data.Models
{
    public enum PurchaseReturnStatus
    {
        [Description("مسودة")]
        Draft,
        [Description("تم الترحيل")]
        Posted,
        [Description("ملغي")]
        Cancelled,

        // ======================= بداية الإضافة =======================
        // هذه الحالات ستستخدم في الفاتورة الأصلية لتتبع حالة الإرجاع
        [Description("لم يتم الإرجاع")]
        NotReturned,
        [Description("مرتجع جزئي")]
        PartiallyReturned,
        [Description("مرتجع بالكامل")]
        FullyReturned
        // ======================== نهاية الإضافة ========================
    }

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
        public string ReturnNumber { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        [Required]
        public int PurchaseId { get; set; }
        public virtual Purchase Purchase { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalReturnValue { get; set; }

        public string? Notes { get; set; }

        [Required]
        public PurchaseReturnStatus Status { get; set; } = PurchaseReturnStatus.Draft;

        public virtual ICollection<PurchaseReturnItem> PurchaseReturnItems { get; set; }
    }
}