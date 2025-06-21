// Data/Models/ShipmentItem.cs
// *** ملف جديد: يمثل سطراً واحداً (بنداً) في الشحنة ***
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class ShipmentItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ShipmentId { get; set; }
        public virtual Shipment Shipment { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        public int Quantity { get; set; } // الكمية المشحونة
    }
}