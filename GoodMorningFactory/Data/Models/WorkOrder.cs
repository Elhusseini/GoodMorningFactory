// Data/Models/WorkOrder.cs
// *** تحديث: تم حذف حقل MaterialsConsumed غير المرن ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public enum WorkOrderStatus { Planned, InProgress, Completed, OnHold, Cancelled }

    public class WorkOrder
    {
        public WorkOrder()
        {
            WorkOrderMaterials = new HashSet<WorkOrderMaterial>();
            WorkOrderScraps = new HashSet<WorkOrderScrap>(); // تهيئة القائمة الجديدة
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string WorkOrderNumber { get; set; }

        [Required]
        public int FinishedGoodId { get; set; }
        public virtual Product FinishedGood { get; set; }

        [Required]
        public int QuantityToProduce { get; set; }

        public int QuantityProduced { get; set; }

        public int QuantityScrapped { get; set; } // الكمية الإجمالية التالفة


        [Required]
        public DateTime PlannedStartDate { get; set; }

        [Required]
        public DateTime PlannedEndDate { get; set; }

        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }

        [Required]
        public WorkOrderStatus Status { get; set; }

        public virtual ICollection<WorkOrderMaterial> WorkOrderMaterials { get; set; }
        public virtual ICollection<WorkOrderScrap> WorkOrderScraps { get; set; } // علاقة جديدة

    }
}