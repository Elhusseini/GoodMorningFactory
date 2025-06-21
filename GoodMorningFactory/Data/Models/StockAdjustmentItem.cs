// Data/Models/StockAdjustmentItem.cs
// *** الكود الكامل والمؤكد ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class StockAdjustmentItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StockAdjustmentId { get; set; }
        public virtual StockAdjustment StockAdjustment { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        public int SystemQuantity { get; set; }

        [Required]
        public int CountedQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitCost { get; set; }

        [NotMapped]
        public int Difference => CountedQuantity - SystemQuantity;
    }
}