// Data/Models/PurchaseReturnItem.cs
// *** ملف جديد: يمثل سطراً واحداً (بنداً) في مرتجع المشتريات ***
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class PurchaseReturnItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PurchaseReturnId { get; set; }
        public virtual PurchaseReturn PurchaseReturn { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        public int Quantity { get; set; } // الكمية المرتجعة
    }
}