// Core/Services/IReportsService.cs

using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// واجهة تعرف العقد الخاص بخدمة التقارير.
    /// تحدد جميع الوظائف اللازمة لجلب البيانات لمختلف تقارير النظام.
    /// </summary>
    public interface IReportsService
    {
        #region تقارير المبيعات والمشتريات والمخزون
        Task<List<Sale>> GetSalesReportDataAsync(DateTime fromDate, DateTime toDate);
        Task<List<Purchase>> GetPurchasesReportDataAsync(DateTime fromDate, DateTime toDate);
        Task<List<InventoryViewModel>> GetInventoryReportDataAsync();
        #endregion

        #region التقارير المالية
        Task<List<Account>> GetAccountsForFilterAsync();
        Task<List<GeneralLedgerItemViewModel>> GetGeneralLedgerReportAsync(int accountId, DateTime fromDate, DateTime toDate);
        Task<List<TrialBalanceItemViewModel>> GetTrialBalanceReportAsync(DateTime toDate);
        Task<IncomeStatementViewModel> GetIncomeStatementReportAsync(DateTime fromDate, DateTime toDate);
        Task<List<BalanceSheetAccountViewModel>> GetBalanceSheetAssetsAsync(DateTime toDate);
        Task<(List<BalanceSheetAccountViewModel> Liabilities, List<BalanceSheetAccountViewModel> Equity)> GetBalanceSheetLiabilitiesAndEquityAsync(DateTime toDate);
        Task<List<CashFlowItemViewModel>> GetCashFlowReportAsync(DateTime fromDate, DateTime toDate);
        #endregion

        #region تقارير الإنتاج
        Task<List<WorkOrder>> GetWorkOrdersReportAsync(DateTime fromDate, DateTime toDate, WorkOrderStatus? status);
        Task<List<ProductionCostReportViewModel>> GetProductionCostReportAsync(DateTime fromDate, DateTime toDate);
        Task<List<MaterialConsumptionReportViewModel>> GetMaterialConsumptionReportAsync(int workOrderId);
        Task<List<ProductionEfficiencyReportViewModel>> GetProductionEfficiencyReportAsync(DateTime fromDate, DateTime toDate);
        Task<List<ScrapReportViewModel>> GetScrapReportAsync(DateTime fromDate, DateTime toDate);
        Task<List<WorkOrder>> GetCompletedWorkOrdersForFilterAsync();
        #endregion

        #region تقارير أخرى
        Task<List<CostCenter>> GetCostCentersForFilterAsync();
        Task<List<CostCenterReportViewModel>> GetCostCenterReportAsync(DateTime fromDate, DateTime toDate, int? costCenterId);
        Task<List<Budget>> GetBudgetsForFilterAsync();
        Task<List<BudgetVsActualViewModel>> GetBudgetVsActualReportAsync(int budgetId, int month, int year);
        #endregion
    }
}