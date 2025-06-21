// Data/Models/PurchaseRequisitionItem.cs
// *** الكود الكامل والمصحح ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class PurchaseRequisitionItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PurchaseRequisitionId { get; set; }
        public virtual PurchaseRequisition PurchaseRequisition { get; set; }

        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }

        // ======================= بداية الإصلاح =======================
        // تم تهيئة الحقل بقيمة نص فارغ لمنع خطأ NOT NULL في قاعدة البيانات
        [Required]
        public string Description { get; set; } = string.Empty;
        // ======================== نهاية الإصلاح ========================

        [Required]
        public decimal Quantity { get; set; }

        public string? UnitOfMeasure { get; set; }
    }
}