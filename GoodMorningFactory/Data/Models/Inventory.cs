// Data/Models/Inventory.cs
// *** تحديث: تمت إضافة حقل لربط المخزون بالمخزن المحدد ***
using System;
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }
        // --- بداية التحديث: إضافة هوية المخزن ---
        [Required]
        public int WarehouseId { get; set; }
        public virtual Warehouse Warehouse { get; set; }
        // --- نهاية التحديث ---

        [Required]
        public int ProductId { get; set; } // مفتاح أجنبي للمنتج
        public virtual Product Product { get; set; }

        [Required]
        public int Quantity { get; set; } // الكمية المتوفرة في المخزون
        // --- بداية التحديث: إضافة حقل جديد ---
        public int QuantityReserved { get; set; } // الكمية المحجوزة لأوامر البيع
        // --- نهاية التحديث ---
        public int ReorderLevel { get; set; } // حد إعادة الطلب (لإصدار تنبيهات عند انخفاض المخزون)

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}