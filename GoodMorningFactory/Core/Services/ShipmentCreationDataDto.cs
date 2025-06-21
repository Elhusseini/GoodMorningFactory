using GoodMorningFactory.UI.ViewModels;
using System;
using System.Collections.Generic;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// كائن لنقل البيانات اللازمة لإنشاء شحنة جديدة من الخدمة إلى الـ ViewModel.
    /// </summary>
    public class ShipmentCreationDataDto
    {
        public string SalesOrderNumber { get; set; }
        public DateTime ShipmentDate { get; set; }
        public List<ShipmentItemViewModel> ItemsToShip { get; set; }

        public ShipmentCreationDataDto()
        {
            ItemsToShip = new List<ShipmentItemViewModel>();
        }
    }
}
