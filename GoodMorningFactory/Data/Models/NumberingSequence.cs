// Data/Models/NumberingSequence.cs
// *** ملف جديد: يمثل إعدادات الترقيم التلقائي لكل مستند ***
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public enum DocumentType
    {
        SalesQuotation,
        SalesOrder,
        SalesInvoice,
        PurchaseOrder,
        WorkOrder,
        Customer,
        Supplier
    }

    public class NumberingSequence
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DocumentType DocumentType { get; set; }

        [MaxLength(10)]
        public string Prefix { get; set; } // البادئة (مثال: INV-)

        public int LastNumber { get; set; } // آخر رقم تم استخدامه

        public int NumberOfDigits { get; set; } // عدد الخانات (مثال: 5 لـ 00001)
    }
}