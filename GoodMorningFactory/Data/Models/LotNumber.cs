// Data/Models/LotNumber.cs
// ملف جديد: يمثل دفعة واحدة من منتج معين
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    [Table("LotNumbers")]
    public class LotNumber
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        [MaxLength(100)]
        public string Value { get; set; } // قيمة رقم الدفعة

        [Required]
        public int CurrentQuantity { get; set; } // الكمية المتبقية في هذه الدفعة

        public DateTime? ExpiryDate { get; set; } // تاريخ الصلاحية

        [Required]
        public int StorageLocationId { get; set; }
        public virtual StorageLocation StorageLocation { get; set; }

        public int? GoodsReceiptNoteItemId { get; set; }
    }
}
