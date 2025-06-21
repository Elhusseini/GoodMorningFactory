// Data/Models/WorkOrderLaborLog.cs
// *** ملف جديد: يمثل سجل تكلفة عمالة واحد على أمر عمل ***
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class WorkOrderLaborLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WorkOrderId { get; set; }
        public virtual WorkOrder WorkOrder { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal HoursWorked { get; set; } // عدد الساعات الفعلية

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal HourlyRate { get; set; } // تكلفة الساعة

        // خاصية محسوبة لتكلفة هذا السجل
        public decimal TotalCost => HoursWorked * HourlyRate;

        public string Description { get; set; }
    }
}
