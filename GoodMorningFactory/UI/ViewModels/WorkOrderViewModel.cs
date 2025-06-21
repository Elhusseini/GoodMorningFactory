using GoodMorningFactory.Data.Models;
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class WorkOrderViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string WorkOrderNumber { get; set; }
        public string FinishedGoodName { get; set; }
        public int QuantityToProduce { get; set; }
        public int QuantityProduced { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public WorkOrderStatus Status { get; set; }
    }
}