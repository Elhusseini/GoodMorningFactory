// Data/Models/Payslip.cs
// *** ملف جديد: يمثل قسيمة الراتب التفصيلية لكل موظف ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class Payslip
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PayrollId { get; set; }
        public virtual Payroll Payroll { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BasicSalary { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Allowances { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Deductions { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetSalary { get; set; }
    }
}