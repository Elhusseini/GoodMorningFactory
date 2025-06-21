// GoodMorning/Data/Models/InventoryCount.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum InventoryCountStatus
    {
        [Description("مخطط له")]
        Planned,
        [Description("قيد التنفيذ")]
        InProgress,
        [Description("مكتمل")]
        Completed,
        [Description("ملغي")]
        Cancelled
    }

    [Table("InventoryCounts")]
    public class InventoryCount
    {
        public InventoryCount()
        {
            Items = new HashSet<InventoryCountItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string CountReferenceNumber { get; set; }

        [Required]
        public DateTime CountDate { get; set; }

        [Required]
        public int WarehouseId { get; set; }
        public virtual Warehouse Warehouse { get; set; }

        // ---  بداية التعديل الرئيسي  ---
        // تم تغيير الخاصية لتقبل القيمة null في النموذج لتجنب الأخطاء
        public string Notes { get; set; } = string.Empty;
        // ---  نهاية التعديل الرئيسي  ---

        [Required]
        public InventoryCountStatus Status { get; set; }

        public int? ResponsibleUserId { get; set; }
        public virtual User ResponsibleUser { get; set; }

        public virtual ICollection<InventoryCountItem> Items { get; set; }
    }
}