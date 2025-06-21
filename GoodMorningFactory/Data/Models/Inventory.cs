// Data/Models/Inventory.cs
// *** تحديث جوهري: تم تغيير الربط من المخزن الرئيسي إلى الموقع الفرعي ***
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        // --- بداية التعديل: تغيير الربط ---
        [Required]
        public int StorageLocationId { get; set; }
        public virtual StorageLocation StorageLocation { get; set; }
        // --- نهاية التعديل ---

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        public int Quantity { get; set; } // الكمية الفعلية في هذا الموقع

        public int QuantityReserved { get; set; } // الكمية المحجوزة في هذا الموقع

        public int ReorderLevel { get; set; }

        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
