// Data/Models/StorageLocation.cs
// *** ملف جديد: يمثل موقع تخزين فرعي (رف، منطقة) داخل مخزن رئيسي ***
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
        public string Name { get; set; } // مثال: "الرف A-01" أو "منطقة الاستلام"

        [MaxLength(50)]
        public string Code { get; set; } // كود فريد للموقع داخل المخزن

        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
