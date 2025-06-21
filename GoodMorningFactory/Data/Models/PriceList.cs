// Data/Models/PriceList.cs
// *** الكود الكامل والمؤكد بعد المراجعة ***
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace GoodMorningFactory.Data.Models
{
    public class PriceList
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // اسم قائمة الأسعار (مثال: سعر التجزئة)

        public string? Description { get; set; }

        // ======================= بداية التعديل المؤكد =======================

        /// <summary>
        /// خاصية الربط التي تحتوي على جميع أسعار المنتجات
        /// المرتبطة بقائمة الأسعار هذه.
        /// ضرورية لعمل .Include() في Entity Framework.
        /// </summary>
        public virtual ICollection<ProductPrice> ProductPrices { get; set; }

        // ======================== نهاية التعديل المؤكد ========================

        /// <summary>
        /// المُنشئ لتهيئة قائمة أسعار المنتجات
        /// وضمان أنها لا تكون فارغة (null).
        /// </summary>
        public PriceList()
        {
            ProductPrices = new Collection<ProductPrice>();
        }
    }
}