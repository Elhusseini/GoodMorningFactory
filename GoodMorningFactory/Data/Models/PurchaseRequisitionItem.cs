// Data/Models/PurchaseRequisitionItem.cs
// *** ملف جديد: يمثل سطراً واحداً (بنداً) في طلب الشراء ***
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
        public virtual Product Product { get; set; } // إضافة هذه العلاقة

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        public string? UnitOfMeasure { get; set; }
    }
}