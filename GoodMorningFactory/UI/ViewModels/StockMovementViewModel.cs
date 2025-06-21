// UI/ViewModels/StockMovementViewModel.cs
// *** ملف جديد: ViewModel خاص بعرض سجل حركات المخزون ***
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class StockMovementViewModel
    {
        public DateTime Date { get; set; }
        public string TransactionType { get; set; }
        public string ReferenceNumber { get; set; }
        public string ProductName { get; set; }
        public int QuantityIn { get; set; } // الكمية الداخلة
        public int QuantityOut { get; set; } // الكمية الخارجة
        public string User { get; set; } // المستخدم الذي قام بالحركة
    }
}