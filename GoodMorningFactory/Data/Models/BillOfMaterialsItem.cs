// Data/Models/BillOfMaterialsItem.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class BillOfMaterialsItem
    {
        [Key]
        public int Id { get; set; }

        // --- العلاقات ---
        [Required]
        public int BillOfMaterialsId { get; set; }
        public virtual BillOfMaterials BillOfMaterials { get; set; }

        [Required]
        public int RawMaterialId { get; set; } // المادة الخام
        public virtual Product RawMaterial { get; set; }

        // --- بيانات البند ---
        [Required]
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; } // الكمية المطلوبة

        [Column(TypeName = "decimal(5, 2)")]
        public decimal ScrapPercentage { get; set; } = 0; // نسبة الهالك المتوقعة
    }
}
