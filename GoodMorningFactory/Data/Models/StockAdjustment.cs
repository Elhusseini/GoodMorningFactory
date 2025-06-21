// Data/Models/StockAdjustment.cs
// *** ملف جديد: يمثل رأس عملية تعديل المخزون ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class StockAdjustment
    {
        public StockAdjustment()
        {
            StockAdjustmentItems = new HashSet<StockAdjustmentItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReferenceNumber { get; set; }

        [Required]
        public DateTime AdjustmentDate { get; set; }

        [Required]
        public int WarehouseId { get; set; }
        public virtual Warehouse Warehouse { get; set; }

        public string Reason { get; set; }

        public virtual ICollection<StockAdjustmentItem> StockAdjustmentItems { get; set; }
    }
}
