// Data/Models/GoodsReceiptNoteItem.cs
// *** ملف جديد: يمثل سطراً واحداً (بنداً) في مذكرة الاستلام ***
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class GoodsReceiptNoteItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GoodsReceiptNoteId { get; set; }
        public virtual GoodsReceiptNote GoodsReceiptNote { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        public int QuantityReceived { get; set; } // الكمية المستلمة
    }
}