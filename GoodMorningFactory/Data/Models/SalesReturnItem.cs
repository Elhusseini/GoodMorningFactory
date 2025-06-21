// Data/Models/SalesReturnItem.cs
// *** الكود الكامل لنموذج بنود مرتجعات المبيعات ***
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class SalesReturnItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SalesReturnId { get; set; }
        public virtual SalesReturn SalesReturn { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        public int Quantity { get; set; } // الكمية المرتجعة
    }
}