// GoodMorningFactory/Data/Models/AssetDepreciation.cs
// *** ملف جديد: يمثل سجل إهلاك لأصل ثابت في فترة معينة ***
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoodMorningFactory.Data.Models
{
    [Table("AssetDepreciations")]
    public class AssetDepreciation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FixedAssetId { get; set; }
        public virtual FixedAsset FixedAsset { get; set; }

        [Required]
        public int JournalVoucherId { get; set; }
        public virtual JournalVoucher JournalVoucher { get; set; }

        [Required]
        public DateTime DepreciationDate { get; set; } // تاريخ الفترة التي تم احتساب الإهلاك لها

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; } // مبلغ الإهلاك لهذه الفترة
    }
}