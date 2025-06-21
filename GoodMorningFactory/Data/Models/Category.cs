// GoodMorningFactory/Data/Models/Category.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    /// <summary>
    /// يمثل هذا الكلاس جدول الفئات (Categories) في قاعدة البيانات.
    /// يدعم الهيكل الشجري من خلال علاقة ذاتية (self-referencing relationship).
    /// </summary>
    [Table("Categories")]
    public class Category
    {
        public Category()
        {
            Products = new HashSet<Product>();
            ChildCategories = new HashSet<Category>();
        }

        [Key]
        public int Id { get; set; }

        // --- بداية الإضافة: إضافة حقل كود الفئة ---
        /// <summary>
        /// كود مختصر وفريد للفئة (مثال: FNT للأثاث).
        /// يستخدم كبادئة في توليد أكواد المنتجات.
        /// </summary>
        [Required(ErrorMessage = "كود الفئة مطلوب.")]
        [MaxLength(10)]
        public string CategoryCode { get; set; }
        // --- نهاية الإضافة ---

        [Required(ErrorMessage = "اسم الفئة مطلوب.")]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // مفتاح خارجي يشير إلى الفئة الأم (اختياري)
        public int? ParentCategoryId { get; set; }
        [ForeignKey("ParentCategoryId")]
        public virtual Category ParentCategory { get; set; }

        // مجموعة الفئات الفرعية التابعة لهذه الفئة
        public virtual ICollection<Category> ChildCategories { get; set; }

        // مجموعة المنتجات التابعة لهذه الفئة
        public virtual ICollection<Product> Products { get; set; }
    }
}