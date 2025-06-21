// GoodMorning/UI/ViewModels/StockMovementViewModel.cs
// *** ملف جديد: ViewModel مركزي وموحد لعرض حركات المخزون ***
using GoodMorningFactory.Data.Models;
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class StockMovementViewModel
    {
        public DateTime Date { get; set; }
        public StockMovementType MovementType { get; set; }
        public string ReferenceNumber { get; set; }
        public string ProductName { get; set; }
        public string WarehouseName { get; set; }
        public string StorageLocationName { get; set; }
        public int QuantityIn { get; set; }
        public int QuantityOut { get; set; }
        public string UserName { get; set; }
    }
}
