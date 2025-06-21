// Data/Models/BillOfMaterialsItem.cs
// *** ملف جديد: يمثل سطراً واحداً (بنداً) في قائمة المكونات ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class BillOfMaterialsItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BillOfMaterialsId { get; set; }
        public virtual BillOfMaterials BillOfMaterials { get; set; }

        [Required]
        public int RawMaterialId { get; set; } // المادة الخام
        public virtual Product RawMaterial { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; } // الكمية المطلوبة
    }
}