// Data/Models/QualityCheck.cs
// *** ملف جديد: يمثل سجل عملية فحص جودة كاملة ***

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    /// <summary>
    /// تحديد سياق عملية الفحص
    /// يوضح متى تمت عملية الفحص في دورة حياة المنتج
    /// </summary>
    public enum QualityCheckType
    {
        /// <summary>
        /// فحص عند استلام المواد الخام أو المنتجات من الموردين
        /// </summary>
        Receiving,
        
        /// <summary>
        /// فحص بعد عملية التصنيع للمنتجات المصنعة داخلياً
        /// </summary>
        Production
    }

    /// <summary>
    /// حالة عملية الفحص الإجمالية
    /// تحدد نتيجة الفحص النهائية للمنتج
    /// </summary>
    public enum QualityStatus
    {
        /// <summary>
        /// الفحص قيد الإجراء أو لم يكتمل بعد
        /// </summary>
        Pending,
        
        /// <summary>
        /// تم قبول المنتج بعد اجتيازه معايير الجودة
        /// </summary>
        Approved,
        
        /// <summary>
        /// تم رفض المنتج لعدم مطابقته لمعايير الجودة
        /// </summary>
        Rejected
    }

    /// <summary>
    /// نموذج يمثل عملية فحص جودة كاملة
    /// يحتفظ بتفاصيل الفحص ونتائجه وعلاقاته مع المنتجات وأوامر العمل
    /// </summary>
    public class QualityCheck
    {
        /// <summary>
        /// منشئ النموذج
        /// يقوم بتهيئة مجموعة نتائج الفحص كمجموعة فارغة
        /// </summary>
        public QualityCheck()
        {
            Results = new HashSet<QualityCheckResult>();
        }

        /// <summary>
        /// المعرف الفريد لعملية الفحص
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// تاريخ ووقت إجراء الفحص
        /// يستخدم للتتبع وإنشاء التقارير الزمنية
        /// </summary>
        [Required]
        public DateTime CheckDate { get; set; }

        /// <summary>
        /// نوع الفحص (عند الاستلام أو بعد الإنتاج)
        /// يحدد سياق الفحص وقواعد التحقق المطبقة
        /// </summary>
        [Required]
        public QualityCheckType CheckType { get; set; }

        /// <summary>
        /// النتيجة الإجمالية للفحص
        /// تحدد ما إذا كان المنتج مقبولاً أو مرفوضاً
        /// </summary>
        [Required]
        public QualityStatus OverallStatus { get; set; }

        #region علاقات قاعدة البيانات

        /// <summary>
        /// معرف المنتج الذي تم فحصه
        /// علاقة إجبارية لأن كل فحص يجب أن يرتبط بمنتج
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// المنتج الذي تم فحصه
        /// علاقة الملاحة مع جدول المنتجات
        /// </summary>
        public virtual Product Product { get; set; }

        /// <summary>
        /// معرف بند فاتورة الشراء
        /// يُستخدم فقط عند فحص المواد المستلمة (CheckType = Receiving)
        /// </summary>
        public int? PurchaseItemId { get; set; }

        /// <summary>
        /// بند فاتورة الشراء المرتبط بالفحص
        /// علاقة اختيارية تستخدم فقط مع فحوصات الاستلام
        /// </summary>
        public virtual PurchaseItem? PurchaseItem { get; set; }

        /// <summary>
        /// معرف أمر العمل
        /// يُستخدم فقط عند فحص المنتجات المصنعة (CheckType = Production)
        /// </summary>
        public int? WorkOrderId { get; set; }

        /// <summary>
        /// أمر العمل المرتبط بالفحص
        /// علاقة اختيارية تستخدم فقط مع فحوصات الإنتاج
        /// </summary>
        public virtual WorkOrder? WorkOrder { get; set; }

        /// <summary>
        /// نتائج الفحص التفصيلية
        /// مجموعة من نتائج الفحص لكل معيار تم فحصه
        /// علاقة واحد لكثير مع جدول نتائج الفحص
        /// </summary>
        public virtual ICollection<QualityCheckResult> Results { get; set; }

        #endregion
    }
}