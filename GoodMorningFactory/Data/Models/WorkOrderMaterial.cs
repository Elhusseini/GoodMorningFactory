// Data/Models/WorkOrderMaterial.cs
// *** ملف جديد: يمثل المواد المستهلكة في أمر عمل معين ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class WorkOrderMaterial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WorkOrderId { get; set; }
        public virtual WorkOrder WorkOrder { get; set; }

        [Required]
        public int RawMaterialId { get; set; } // المادة الخام
        public virtual Product RawMaterial { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 4)")]
        public decimal QuantityConsumed { get; set; } // الكمية المستهلكة
    }
}