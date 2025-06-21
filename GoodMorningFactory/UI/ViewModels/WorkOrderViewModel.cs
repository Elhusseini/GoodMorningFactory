// UI/ViewModels/WorkOrderViewModel.cs
// *** ملف جديد: ViewModel لعرض بيانات أوامر العمل بشكل متقدم ***
using GoodMorningFactory.Data.Models;

namespace GoodMorningFactory.UI.ViewModels
{
    public class WorkOrderViewModel
    {
        // الكائن الأصلي لأمر العمل
        public WorkOrder Order { get; set; }

        // خاصية محسوبة لمعرفة ما إذا كانت المواد قد تم استهلاكها
        public bool AreMaterialsConsumed { get; set; }

        // الخصائص المعروضة في الواجهة
        public string WorkOrderNumber => Order.WorkOrderNumber;
        public Product FinishedGood => Order.FinishedGood;
        public int QuantityToProduce => Order.QuantityToProduce;
        public int QuantityProduced => Order.QuantityProduced;
        public System.DateTime PlannedStartDate => Order.PlannedStartDate;
        public WorkOrderStatus Status => Order.Status;
    }
}
