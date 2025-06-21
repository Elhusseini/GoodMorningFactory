// Data/Models/StockAdjustmentItem.cs
// *** ملف جديد: يمثل سطراً واحداً في عملية تعديل المخزون ***
using System.ComponentModel.DataAnnotations;

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

        public int Difference => CountedQuantity - SystemQuantity;
    }
}
