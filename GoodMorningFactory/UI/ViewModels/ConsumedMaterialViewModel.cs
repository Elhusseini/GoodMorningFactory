// UI/ViewModels/ConsumedMaterialViewModel.cs
// *** ملف جديد: ViewModel لعرض المواد التي تم صرفها بالفعل ***
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ConsumedMaterialViewModel
    {
        public string MaterialName { get; set; }
        public decimal QuantityConsumed { get; set; }
        public DateTime ConsumptionDate { get; set; } // يمكن جلبها من تاريخ القيد إذا تم ربطها
    }
}