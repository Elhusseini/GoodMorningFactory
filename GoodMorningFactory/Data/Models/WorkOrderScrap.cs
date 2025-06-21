// Data/Models/WorkOrderScrap.cs
// *** ملف جديد: يمثل سجل عملية هدر في أمر عمل معين ***
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class WorkOrderScrap
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WorkOrderId { get; set; }
        public virtual WorkOrder WorkOrder { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        public int Quantity { get; set; } // الكمية التالفة

        public string? Reason { get; set; } // سبب الهدر
    }
}