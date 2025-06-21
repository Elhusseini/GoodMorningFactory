// GoodMorning/Data/Models/InventoryCountItem.cs
// *** ملف جديد: يمثل سطراً واحداً في عملية الجرد ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    [Table("InventoryCountItems")]
    public class InventoryCountItem
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public int InventoryCountId { get; set; }
        public virtual InventoryCount InventoryCount { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        public int StorageLocationId { get; set; }
        public virtual StorageLocation StorageLocation { get; set; }

        [Required]
        public int SystemQuantity { get; set; } // الكمية المسجلة بالنظام عند بدء الجرد

        [Required]
        public int CountedQuantity { get; set; } // الكمية الفعلية التي تم عدها

        // خاصية محسوبة لا يتم تخزينها في قاعدة البيانات
        [NotMapped]
        public int Difference => CountedQuantity - SystemQuantity;
    }
}
