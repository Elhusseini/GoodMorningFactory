// Data/Models/GoodsReceiptNote.cs
// *** الكود الكامل والنهائي ***
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        /// <summary>
        /// معرّف فاتورة المشتريات التي تم إنشاؤها من سند الاستلام هذا.
        /// قيمته تكون null إذا لم تتم الفوترة بعد.
        /// </summary>
        public int? PurchaseId { get; set; }
        public virtual Purchase Purchase { get; set; }

        /// <summary>
        /// خاصية للقراءة فقط لتحديد ما إذا كان قد تمت فوترة السند.
        /// هذه الخاصية لا تُحفظ في قاعدة البيانات وهي موجودة لتسهيل الربط في الواجهة.
        /// </summary>
        [NotMapped]
        public bool IsInvoiced => PurchaseId.HasValue;

        public virtual ICollection<GoodsReceiptNoteItem> GoodsReceiptNoteItems { get; set; }
    }
}