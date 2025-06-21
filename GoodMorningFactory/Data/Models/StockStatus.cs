// GoodMorningFactory/Data/Models/StockStatus.cs
using System.ComponentModel;

namespace GoodMorningFactory.Data.Models
{
    /// <summary>
    /// يمثل حالات المخزون المختلفة للمنتج.
    /// تم نقله إلى هنا ليكون متاحاً على مستوى المشروع.
    /// </summary>
    public enum StockStatus
    {
        [Description("متوفر")]
        Available,
        [Description("كمية منخفضة")]
        LowStock,
        [Description("نفد المخزون")]
        OutOfStock
    }
}