using GoodMorningFactory.Data.Models;
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel يمثل شحنة واحدة للعرض في الجدول.
    /// </summary>
    public class ShipmentViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string ShipmentNumber { get; set; }
        public DateTime ShipmentDate { get; set; }
        public string SalesOrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string Carrier { get; set; }
        public string TrackingNumber { get; set; }
        public ShipmentStatus Status { get; set; }
    }
}
