using System;
using System.Collections.Generic;
using System.ComponentModel; // <-- تأكد من وجود هذا السطر
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public enum ShipmentStatus
    {
        [Description("قيد التجهيز")]
        Preparing,

        [Description("تم الشحن")]
        Shipped,

        [Description("تم التسليم")]
        Delivered
    }

    public class Shipment
    {
        public Shipment()
        {
            ShipmentItems = new HashSet<ShipmentItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ShipmentNumber { get; set; }

        [Required]
        public DateTime ShipmentDate { get; set; }

        [Required]
        public int SalesOrderId { get; set; }
        public virtual SalesOrder SalesOrder { get; set; }

        public string? Carrier { get; set; } // شركة الشحن

        public string? TrackingNumber { get; set; } // رقم التتبع

        [Required]
        public ShipmentStatus Status { get; set; }

        public virtual ICollection<ShipmentItem> ShipmentItems { get; set; }
    }
}