// Data/Models/SerialNumber.cs
// ملف جديد: يمثل رقم تسلسلي واحد لمنتج معين
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum SerialNumberStatus { InStock, Shipped, Consumed, Scrapped }

    [Table("SerialNumbers")]
    public class SerialNumber
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        [MaxLength(100)]
        public string Value { get; set; } // قيمة الرقم التسلسلي

        [Required]
        public int StorageLocationId { get; set; }
        public virtual StorageLocation StorageLocation { get; set; }

        [Required]
        public SerialNumberStatus Status { get; set; } = SerialNumberStatus.InStock;

        public int? GoodsReceiptNoteItemId { get; set; } // لتتبع مصدر الاستلام
    }
}