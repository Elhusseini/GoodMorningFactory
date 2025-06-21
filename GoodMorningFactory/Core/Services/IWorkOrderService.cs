// GoodMorningFactory/Core/Services/IWorkOrderService.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    public interface IWorkOrderService
    {
        Task<PaginatedResult<WorkOrder>> GetWorkOrdersAsync(WorkOrderFilterCriteria criteria);
        Task<List<object>> GetStatusFiltersAsync();
        Task<AddEditWorkOrderDto> GetInitialDataForAddEditWindowAsync(int? workOrderId, int? salesOrderItemId);
        Task<List<RequiredMaterialViewModel>> GetRequiredMaterialsForProductAsync(int productId, int quantity);
        Task<int> SaveWorkOrderAsync(WorkOrder order, bool isNew);
        Task UpdateWorkOrderStatusAsync(int workOrderId, WorkOrderStatus newStatus);
        Task<MaterialConsumptionDataDto> GetDataForMaterialConsumptionAsync(int workOrderId);
        Task ConsumeMaterialsForWorkOrderAsync(int workOrderId, IEnumerable<RequiredMaterialViewModel> itemsToConsume);
        Task<ProductionReportDataDto> GetDataForProductionReportAsync(int workOrderId);
        Task ReportProductionAsync(int workOrderId, int producedQuantity, int scrappedQuantity, string scrapReason);
        Task<RecordLaborDataDto> GetDataForLaborRecordAsync(int workOrderId);
        Task RecordLaborAsync(int workOrderId, int employeeId, DateTime workDate, decimal hoursWorked, string description);

        // --- بداية الإضافة: دالة جديدة لإغلاق أمر العمل ---
        /// <summary>
        /// يقوم بتنفيذ عملية إغلاق أمر العمل محاسبياً وتشغيلياً.
        /// </summary>
        Task CloseWorkOrderAsync(int workOrderId);
        // --- نهاية الإضافة ---
    }

    public class WorkOrderFilterCriteria
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string SearchText { get; set; }
        public WorkOrderStatus? Status { get; set; }
    }

    public class AddEditWorkOrderDto
    {
        public WorkOrder WorkOrder { get; set; }
        public List<Product> FinishedGoods { get; set; }
        public List<WorkOrderStatus> Statuses { get; set; }
        public List<RequiredMaterialViewModel> RequiredMaterials { get; set; } = new List<RequiredMaterialViewModel>();
        public List<ConsumedMaterialViewModel> ConsumedMaterials { get; set; } = new List<ConsumedMaterialViewModel>();
    }

    public class MaterialConsumptionDataDto
    {
        public string WorkOrderNumber { get; set; }
        public List<RequiredMaterialViewModel> RequiredMaterials { get; set; } = new List<RequiredMaterialViewModel>();
    }

    public class ProductionReportDataDto
    {
        public string WorkOrderNumber { get; set; }
        public string ProductName { get; set; }
        public int OrderedQuantity { get; set; }
        public int PreviouslyProduced { get; set; }
        public int RemainingQuantity { get; set; }
    }

    public class RecordLaborDataDto
    {
        public string WorkOrderNumber { get; set; }
        public List<Employee> ActiveEmployees { get; set; } = new List<Employee>();
    }
}