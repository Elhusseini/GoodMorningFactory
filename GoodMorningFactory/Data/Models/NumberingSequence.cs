// GoodMorning/Data/Models/NumberingSequence.cs
// *** تحديث: تمت إضافة نوع مستند "طلب الشراء" ***
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public enum DocumentType
    {
        [Description("عرض سعر")]
        SalesQuotation,

        [Description("أمر بيع")]
        SalesOrder,

        [Description("فاتورة مبيعات")]
        SalesInvoice,

        [Description("أمر شراء")]
        PurchaseOrder,

        // --- بداية الإضافة ---
        [Description("طلب شراء")]
        PurchaseRequisition,
        // --- نهاية الإضافة ---

        [Description("أمر عمل")]
        WorkOrder,

        [Description("عميل")]
        Customer,

        [Description("مورد")]
        Supplier,

        [Description("قيد يومية")]
        JournalVoucher
    }

    public class NumberingSequence
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DocumentType DocumentType { get; set; }

        [MaxLength(10)]
        public string Prefix { get; set; }

        public int LastNumber { get; set; }

        public int NumberOfDigits { get; set; }
    }
}
