// Data/Models/SalesOrderItem.cs
// *** تحديث: تم إنشاء الكلاس الصحيح لبنود أوامر البيع ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class SalesOrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SalesOrderId { get; set; }
        public virtual SalesOrder SalesOrder { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public string? Description { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Discount { get; set; }
    }
}