// Data/Models/TaxRule.cs
// *** ملف جديد: يمثل قواعد الضريبة المختلفة في النظام ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class TaxRule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // e.g., "VAT 15%", "No Tax"

        [Required]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal Rate { get; set; } // The tax rate as a percentage, e.g., 15.00 for 15%
    }
}
