// Data/Models/GoodsReceiptNote.cs
// *** تحديث: تمت إضافة حقل لتتبع الفوترة ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class GoodsReceiptNote
    {
        public GoodsReceiptNote()
        {
            GoodsReceiptNoteItems = new HashSet<GoodsReceiptNoteItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string GRNNumber { get; set; }

        [Required]
        public DateTime ReceiptDate { get; set; }

        [Required]
        public int PurchaseOrderId { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; }

        public bool IsInvoiced { get; set; } = false;

        public virtual ICollection<GoodsReceiptNoteItem> GoodsReceiptNoteItems { get; set; }
    }
}