// Data/Models/Customer.cs
// *** تحديث: تم جعل الحقول النصية اختيارية ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string CustomerCode { get; set; }

        [Required]
        [MaxLength(200)]
        public string CustomerName { get; set; }

        // *** بداية التصحيح ***
        [MaxLength(100)]
        public string? ContactPerson { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? TaxNumber { get; set; }

        public string? BillingAddress { get; set; }

        public string? ShippingAddress { get; set; }

        public string? DefaultPaymentTerms { get; set; }
        // *** نهاية التصحيح ***

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CreditLimit { get; set; }

        public bool IsActive { get; set; } = true;
    }
}