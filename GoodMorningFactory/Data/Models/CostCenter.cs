// Data/Models/CostCenter.cs
// *** ملف جديد: يمثل مركز تكلفة لتحليل الإيرادات والمصروفات ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    [Table("CostCenters")]
    public class CostCenter
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}