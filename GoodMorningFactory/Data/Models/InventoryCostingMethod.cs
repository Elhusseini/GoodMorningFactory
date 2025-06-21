// GoodMorning/Data/Models/InventoryCostingMethod.cs
// *** ملف جديد: لتعريف أنواع طرق تقييم المخزون ***
using System.ComponentModel;

namespace GoodMorningFactory.Data.Models
{
    public enum InventoryCostingMethod
    {
        [Description("المتوسط المرجح المتحرك (WAC)")]
        WeightedAverage,

        [Description("الوارد أولاً صادر أولاً (FIFO)")]
        FIFO
    }
}
