// Data/Models/ProductPrice.cs
// *** ملف جديد: يمثل سعر منتج معين في قائمة أسعار معينة ***
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public class ProductPrice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        public int PriceListId { get; set; }
        public virtual PriceList PriceList { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
    }
}