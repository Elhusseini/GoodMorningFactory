// GoodMorningFactory/Data/Models/StorageLocation.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    [Table("StorageLocations")]
    public class StorageLocation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WarehouseId { get; set; }
        public virtual Warehouse Warehouse { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string Code { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        // --- بداية الإضافة: خاصية لتحديد الموقع الافتراضي ---
        /// <summary>
        /// تحدد ما إذا كان هذا هو الموقع الافتراضي لاستلام المنتجات النهائية من الإنتاج.
        /// يجب أن يكون هناك موقع افتراضي واحد فقط لكل مخزن رئيسي.
        /// </summary>
        public bool IsDefault { get; set; }
        // --- نهاية الإضافة ---
    }
}