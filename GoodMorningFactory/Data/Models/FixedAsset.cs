// Data/Models/FixedAsset.cs
// *** تحديث: تمت إضافة علاقات الربط المحاسبي (Navigation Properties) ***
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    public enum DepreciationMethod
    {
        [Description("القسط الثابت")]
        StraightLine,
        [Description("القسط المتناقص")]
        DecliningBalance
    }

    [Table("FixedAssets")]
    public class FixedAsset
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string AssetName { get; set; }

        [MaxLength(50)]
        public string AssetCode { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime AcquisitionDate { get; set; } // تاريخ الشراء

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal AcquisitionCost { get; set; } // تكلفة الشراء

        [Required]
        public int UsefulLifeYears { get; set; } // العمر الإنتاجي بالسنوات

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SalvageValue { get; set; } // القيمة التخريدية

        [Required]
        public DepreciationMethod DepreciationMethod { get; set; }

        public bool IsDisposed { get; set; } = false; // هل تم التخلص منه؟
        public DateTime? DisposalDate { get; set; }

        // --- الربط المحاسبي ---
        [Required]
        public int AssetAccountId { get; set; } // حساب الأصل
        public virtual Account AssetAccount { get; set; }

        [Required]
        public int AccumulatedDepreciationAccountId { get; set; } // حساب مجمع الإهلاك
        public virtual Account AccumulatedDepreciationAccount { get; set; }

        [Required]
        public int DepreciationExpenseAccountId { get; set; } // حساب مصروف الإهلاك
        public virtual Account DepreciationExpenseAccount { get; set; }
    }
}
