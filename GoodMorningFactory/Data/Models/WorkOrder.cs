// Data/Models/WorkOrder.cs
// *** تحديث: تمت إضافة وصف عربي لحالات أمر العمل ***
using System;
using System.Collections.Generic;
using System.ComponentModel; // <-- إضافة مهمة
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    // --- بداية التحديث: إضافة الوصف العربي للحالات ---
    public enum WorkOrderStatus
    {
        [Description("مخطط له")]
        Planned,
        [Description("قيد التنفيذ")]
        InProgress,
        [Description("مكتمل")]
        Completed,
        [Description("معلق")]
        OnHold,
        [Description("ملغي")]
        Cancelled
    }
    // --- نهاية التحديث ---

    public class WorkOrder
    {
        public WorkOrder()
        {
            WorkOrderMaterials = new HashSet<WorkOrderMaterial>();
            WorkOrderScraps = new HashSet<WorkOrderScrap>();
            WorkOrderLaborLogs = new HashSet<WorkOrderLaborLog>();
        }

        // --- بيانات أساسية ---
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string WorkOrderNumber { get; set; }

        // --- الربط بأمر البيع ---
        public int? SalesOrderItemId { get; set; }
        public virtual SalesOrderItem SalesOrderItem { get; set; }

        // --- بيانات المنتج والكمية ---
        [Required]
        public int FinishedGoodId { get; set; }
        public virtual Product FinishedGood { get; set; }

        [Required]
        public int QuantityToProduce { get; set; }

        // --- بيانات التخطيط ---
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }

        // --- البيانات الفعلية ---
        public int QuantityProduced { get; set; }
        public int QuantityScrapped { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }

        // --- التكاليف والحالة ---
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalLaborCost { get; set; }

        [Required]
        public WorkOrderStatus Status { get; set; }

        // --- العلاقات ---
        public virtual ICollection<WorkOrderMaterial> WorkOrderMaterials { get; set; }
        public virtual ICollection<WorkOrderScrap> WorkOrderScraps { get; set; }
        public virtual ICollection<WorkOrderLaborLog> WorkOrderLaborLogs { get; set; }
    }
}
