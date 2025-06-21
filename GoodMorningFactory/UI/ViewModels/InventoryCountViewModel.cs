// GoodMorning/UI/ViewModels/InventoryCountViewModel.cs
// *** ملف جديد: ViewModel لعرض بيانات أوامر الجرد في القائمة الرئيسية ***
using GoodMorningFactory.Data.Models;
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class InventoryCountViewModel
    {
        public int Id { get; set; }
        public string CountReferenceNumber { get; set; }
        public DateTime CountDate { get; set; }
        public string WarehouseName { get; set; }
        public InventoryCountStatus Status { get; set; }
        public string ResponsibleUser { get; set; }
    }
}
