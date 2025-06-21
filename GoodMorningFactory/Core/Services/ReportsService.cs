// Core/Services/ReportsService.cs
// *** الكود الكامل والمصحح لخدمة التقارير ***

using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// التنفيذ الفعلي لخدمة التقارير.
    /// تحتوي على كل المنطق الخاص باستعلامات قاعدة البيانات لتوليد التقارير.
    /// </summary>
    public class ReportsService : IReportsService
    {
        #region تقارير المبيعات والمشتريات والمخزون
        public async Task<List<Sale>> GetSalesReportDataAsync(DateTime fromDate, DateTime toDate)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Sales
                    .Where(s => s.SaleDate >= fromDate && s.SaleDate <= toDate)
                    .OrderByDescending(s => s.SaleDate)
                    .ToListAsync();
            }
        }

        public async Task<List<Purchase>> GetPurchasesReportDataAsync(DateTime fromDate, DateTime toDate)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Purchases.Include(p => p.Supplier)
                    .Where(p => p.PurchaseDate >= fromDate && p.PurchaseDate <= toDate)
                    .OrderByDescending(p => p.PurchaseDate)
                    .ToListAsync();
            }
        }

        public async Task<List<InventoryViewModel>> GetInventoryReportDataAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await (from p in db.Products.AsNoTracking().Include(p => p.Category)
                              join i in db.Inventories.AsNoTracking() on p.Id equals i.ProductId into gj
                              from subInv in gj.DefaultIfEmpty()
                              select new InventoryViewModel
                              {
                                  ProductId = p.Id,
                                  ProductCode = p.ProductCode,
                                  ProductName = p.Name,
                                  CategoryName = p.Category.Name ?? "غير مصنف",
                                  QuantityOnHand = subInv == null ? 0 : subInv.Quantity
                              }).ToListAsync();
            }
        }
        #endregion

        #region التقارير المالية
        public async Task<List<Account>> GetAccountsForFilterAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Accounts.OrderBy(a => a.AccountNumber).ToListAsync();
            }
        }

        public async Task<List<GeneralLedgerItemViewModel>> GetGeneralLedgerReportAsync(int accountId, DateTime fromDate, DateTime toDate)
        {
            using (var db = new DatabaseContext())
            {
                decimal openingDebit = await db.JournalVoucherItems.Where(i => i.AccountId == accountId && i.JournalVoucher.VoucherDate < fromDate).SumAsync(i => i.Debit);
                decimal openingCredit = await db.JournalVoucherItems.Where(i => i.AccountId == accountId && i.JournalVoucher.VoucherDate < fromDate).SumAsync(i => i.Credit);
                decimal openingBalance = openingDebit - openingCredit;

                var transactions = await db.JournalVoucherItems
                    .Include(i => i.JournalVoucher)
                    .Where(i => i.AccountId == accountId && i.JournalVoucher.VoucherDate >= fromDate && i.JournalVoucher.VoucherDate <= toDate)
                    .OrderBy(i => i.JournalVoucher.VoucherDate)
                    .ToListAsync();

                var reportItems = new List<GeneralLedgerItemViewModel>();
                decimal currentBalance = openingBalance;

                reportItems.Add(new GeneralLedgerItemViewModel { Date = fromDate.AddDays(-1), Description = "رصيد افتتاحي", Balance = openingBalance });

                foreach (var item in transactions)
                {
                    currentBalance += item.Debit - item.Credit;
                    reportItems.Add(new GeneralLedgerItemViewModel
                    {
                        Date = item.JournalVoucher.VoucherDate,
                        VoucherNumber = item.JournalVoucher.VoucherNumber,
                        Description = item.Description ?? item.JournalVoucher.Description,
                        Debit = item.Debit,
                        Credit = item.Credit,
                        Balance = currentBalance
                    });
                }
                return reportItems;
            }
        }

        public async Task<List<TrialBalanceItemViewModel>> GetTrialBalanceReportAsync(DateTime toDate)
        {
            using (var db = new DatabaseContext())
            {
                var reportItems = new List<TrialBalanceItemViewModel>();
                var accounts = await db.Accounts.ToListAsync();

                foreach (var account in accounts)
                {
                    var totalDebit = await db.JournalVoucherItems
                        .Where(i => i.AccountId == account.Id && i.JournalVoucher.VoucherDate <= toDate)
                        .SumAsync(i => i.Debit);

                    var totalCredit = await db.JournalVoucherItems
                        .Where(i => i.AccountId == account.Id && i.JournalVoucher.VoucherDate <= toDate)
                        .SumAsync(i => i.Credit);

                    decimal balance = totalDebit - totalCredit;

                    if (balance != 0)
                    {
                        reportItems.Add(new TrialBalanceItemViewModel
                        {
                            AccountNumber = account.AccountNumber,
                            AccountName = account.AccountName,
                            DebitBalance = balance > 0 ? balance : 0,
                            CreditBalance = balance < 0 ? -balance : 0
                        });
                    }
                }
                return reportItems;
            }
        }

        public async Task<IncomeStatementViewModel> GetIncomeStatementReportAsync(DateTime fromDate, DateTime toDate)
        {
            var viewModel = new IncomeStatementViewModel { ReportDateRange = $"للفترة من {fromDate:yyyy/MM/dd} إلى {toDate:yyyy/MM/dd}" };
            using (var db = new DatabaseContext())
            {
                var revenueAccounts = await db.Accounts.Where(a => a.AccountType == AccountType.Revenue).ToListAsync();
                foreach (var account in revenueAccounts)
                {
                    var balance = await db.JournalVoucherItems.Where(i => i.AccountId == account.Id && i.JournalVoucher.VoucherDate >= fromDate && i.JournalVoucher.VoucherDate <= toDate).SumAsync(i => i.Credit - i.Debit);
                    if (balance != 0)
                        viewModel.Revenues.Add(new IncomeStatementItemViewModel { AccountName = account.AccountName, Balance = balance });
                }

                var expenseAccounts = await db.Accounts.Where(a => a.AccountType == AccountType.Expense).ToListAsync();
                foreach (var account in expenseAccounts)
                {
                    var balance = await db.JournalVoucherItems.Where(i => i.AccountId == account.Id && i.JournalVoucher.VoucherDate >= fromDate && i.JournalVoucher.VoucherDate <= toDate).SumAsync(i => i.Debit - i.Credit);
                    if (balance != 0)
                        viewModel.Expenses.Add(new IncomeStatementItemViewModel { AccountName = account.AccountName, Balance = balance });
                }
            }
            viewModel.CalculateTotals();
            return viewModel;
        }

        public async Task<List<BalanceSheetAccountViewModel>> GetBalanceSheetAssetsAsync(DateTime toDate)
        {
            using (var db = new DatabaseContext())
            {
                var allAccounts = await db.Accounts.ToListAsync();
                var allTransactions = await db.JournalVoucherItems.Include(i => i.JournalVoucher).Where(i => i.JournalVoucher.VoucherDate <= toDate).ToListAsync();
                // **تصحيح**: تحويل النوع من IEnumerable إلى List ليتوافق مع توقيع الواجهة
                return BuildAccountTree(AccountType.Asset, allAccounts, allTransactions).ToList();
            }
        }

        public async Task<(List<BalanceSheetAccountViewModel> Liabilities, List<BalanceSheetAccountViewModel> Equity)> GetBalanceSheetLiabilitiesAndEquityAsync(DateTime toDate)
        {
            using (var db = new DatabaseContext())
            {
                var allAccounts = await db.Accounts.ToListAsync();
                var allTransactions = await db.JournalVoucherItems.Include(i => i.JournalVoucher).Where(i => i.JournalVoucher.VoucherDate <= toDate).ToListAsync();

                // **تصحيح**: تحويل النوع من IEnumerable إلى List ليتوافق مع توقيع الواجهة
                var liabilities = BuildAccountTree(AccountType.Liability, allAccounts, allTransactions).ToList();
                var equity = BuildAccountTree(AccountType.Equity, allAccounts, allTransactions).ToList();

                decimal netProfitLoss = CalculateNetProfitLoss(db, toDate);
                equity.Add(new BalanceSheetAccountViewModel { AccountName = "صافي الربح (الخسارة) للفترة", Balance = netProfitLoss });

                return (liabilities, equity);
            }
        }

        public async Task<List<CashFlowItemViewModel>> GetCashFlowReportAsync(DateTime fromDate, DateTime toDate)
        {
            using (var db = new DatabaseContext())
            {
                var reportItems = new List<CashFlowItemViewModel>();

                reportItems.Add(new CashFlowItemViewModel { Item = "التدفقات النقدية من الأنشطة التشغيلية", IsTotal = true, IndentLevel = 0 });
                decimal netIncome = await CalculateNetProfitLossForPeriodAsync(db, fromDate, toDate);
                reportItems.Add(new CashFlowItemViewModel { Item = "صافي الدخل", Amount = netIncome, IndentLevel = 1 });
                reportItems.Add(new CashFlowItemViewModel { Item = "تسويات لتحويل صافي الدخل إلى نقدية:", IndentLevel = 1 });
                decimal arChange = await GetAccountChangeAsync(db, AccountType.Asset, "الذمم المدينة", fromDate, toDate);
                reportItems.Add(new CashFlowItemViewModel { Item = "النقص (الزيادة) في الذمم المدينة", Amount = -arChange, IndentLevel = 2 });
                decimal inventoryChange = await GetAccountChangeAsync(db, AccountType.Asset, "المخزون", fromDate, toDate);
                reportItems.Add(new CashFlowItemViewModel { Item = "النقص (الزيادة) في المخزون", Amount = -inventoryChange, IndentLevel = 2 });
                decimal apChange = await GetAccountChangeAsync(db, AccountType.Liability, "الذمم الدائنة", fromDate, toDate);
                reportItems.Add(new CashFlowItemViewModel { Item = "الزيادة (النقص) في الذمم الدائنة", Amount = apChange, IndentLevel = 2 });
                decimal totalOperatingCashFlow = netIncome - arChange - inventoryChange + apChange;
                reportItems.Add(new CashFlowItemViewModel { Item = "صافي النقدية من الأنشطة التشغيلية", Amount = totalOperatingCashFlow, IndentLevel = 0, IsTotal = true });

                return reportItems;
            }
        }

        #endregion

        #region تقارير الإنتاج
        public async Task<List<WorkOrder>> GetWorkOrdersReportAsync(DateTime fromDate, DateTime toDate, WorkOrderStatus? status)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.WorkOrders.Include(wo => wo.FinishedGood).AsQueryable();
                if (status.HasValue)
                {
                    query = query.Where(wo => wo.Status == status);
                }
                query = query.Where(wo => wo.PlannedStartDate >= fromDate && wo.PlannedStartDate <= toDate);
                return await query.OrderByDescending(wo => wo.PlannedStartDate).ToListAsync();
            }
        }

        public async Task<List<ProductionCostReportViewModel>> GetProductionCostReportAsync(DateTime fromDate, DateTime toDate)
        {
            using (var db = new DatabaseContext())
            {
                var reportItems = new List<ProductionCostReportViewModel>();
                var completedWorkOrders = await db.WorkOrders
                    .Include(wo => wo.FinishedGood)
                    .Where(wo => wo.Status == WorkOrderStatus.Completed &&
                                 wo.ActualEndDate >= fromDate && wo.ActualEndDate <= toDate)
                    .ToListAsync();

                foreach (var wo in completedWorkOrders)
                {
                    var consumedMaterials = await db.WorkOrderMaterials
                        .Include(m => m.RawMaterial)
                        .Where(m => m.WorkOrderId == wo.Id)
                        .ToListAsync();
                    decimal totalMaterialCost = consumedMaterials.Sum(m => m.QuantityConsumed * m.RawMaterial.AverageCost);
                    reportItems.Add(new ProductionCostReportViewModel
                    {
                        WorkOrderNumber = wo.WorkOrderNumber,
                        ProductName = wo.FinishedGood.Name,
                        ProducedQuantity = wo.QuantityProduced,
                        CompletionDate = wo.ActualEndDate.Value,
                        TotalMaterialCost = totalMaterialCost,
                        TotalLaborCost = wo.TotalLaborCost
                    });
                }
                return reportItems;
            }
        }

        public async Task<List<MaterialConsumptionReportViewModel>> GetMaterialConsumptionReportAsync(int workOrderId)
        {
            using (var db = new DatabaseContext())
            {
                var reportItems = new List<MaterialConsumptionReportViewModel>();
                var workOrder = await db.WorkOrders.FindAsync(workOrderId);
                if (workOrder == null) return reportItems;

                var bom = await db.BillOfMaterials.Include(b => b.BillOfMaterialsItems).ThenInclude(i => i.RawMaterial)
                            .FirstOrDefaultAsync(b => b.FinishedGoodId == workOrder.FinishedGoodId);
                if (bom == null) return reportItems;

                var consumedMaterials = await db.WorkOrderMaterials
                    .Where(m => m.WorkOrderId == workOrderId)
                    .ToListAsync();

                foreach (var bomItem in bom.BillOfMaterialsItems)
                {
                    decimal plannedQty = bomItem.Quantity * workOrder.QuantityToProduce;
                    decimal actualQty = consumedMaterials
                                        .Where(c => c.RawMaterialId == bomItem.RawMaterialId)
                                        .Sum(c => c.QuantityConsumed);
                    reportItems.Add(new MaterialConsumptionReportViewModel
                    {
                        MaterialName = bomItem.RawMaterial.Name,
                        PlannedQuantity = plannedQty,
                        ActualQuantity = actualQty
                    });
                }
                return reportItems;
            }
        }

        public async Task<List<ProductionEfficiencyReportViewModel>> GetProductionEfficiencyReportAsync(DateTime fromDate, DateTime toDate)
        {
            using (var db = new DatabaseContext())
            {
                var reportItems = new List<ProductionEfficiencyReportViewModel>();
                var completedWorkOrders = await db.WorkOrders
                    .Include(wo => wo.FinishedGood)
                    .Where(wo => wo.Status == WorkOrderStatus.Completed &&
                                 wo.ActualStartDate.HasValue &&
                                 wo.ActualEndDate.HasValue &&
                                 wo.ActualEndDate >= fromDate && wo.ActualEndDate <= toDate)
                    .ToListAsync();

                foreach (var wo in completedWorkOrders)
                {
                    reportItems.Add(new ProductionEfficiencyReportViewModel
                    {
                        WorkOrderNumber = wo.WorkOrderNumber,
                        ProductName = wo.FinishedGood.Name,
                        PlannedDurationDays = (wo.PlannedEndDate - wo.PlannedStartDate).TotalDays,
                        ActualDurationDays = (wo.ActualEndDate.Value - wo.ActualStartDate.Value).TotalDays,
                    });
                }
                return reportItems;
            }
        }

        public async Task<List<ScrapReportViewModel>> GetScrapReportAsync(DateTime fromDate, DateTime toDate)
        {
            using (var db = new DatabaseContext())
            {
                return await db.WorkOrderScraps
                    .Include(s => s.WorkOrder)
                    .Include(s => s.Product)
                    .Where(s => s.WorkOrder.ActualEndDate >= fromDate && s.WorkOrder.ActualEndDate <= toDate)
                    .Select(s => new ScrapReportViewModel
                    {
                        WorkOrderNumber = s.WorkOrder.WorkOrderNumber,
                        ProductName = s.Product.Name,
                        Quantity = s.Quantity,
                        Reason = s.Reason,
                        Date = s.WorkOrder.ActualEndDate.Value
                    })
                    .ToListAsync();
            }
        }

        public async Task<List<WorkOrder>> GetCompletedWorkOrdersForFilterAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.WorkOrders
                        .Where(wo => wo.Status == WorkOrderStatus.Completed)
                        .OrderByDescending(wo => wo.WorkOrderNumber)
                        .ToListAsync();
            }
        }
        #endregion

        #region تقارير أخرى
        public async Task<List<CostCenter>> GetCostCentersForFilterAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.CostCenters.Where(c => c.IsActive).ToListAsync();
            }
        }

        public async Task<List<CostCenterReportViewModel>> GetCostCenterReportAsync(DateTime fromDate, DateTime toDate, int? costCenterId)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.JournalVoucherItems
                              .Include(i => i.Account)
                              .Include(i => i.CostCenter)
                              .Where(i => i.JournalVoucher.VoucherDate >= fromDate && i.JournalVoucher.VoucherDate <= toDate)
                              .Where(i => i.Account.AccountType == AccountType.Revenue || i.Account.AccountType == AccountType.Expense);

                if (costCenterId.HasValue && costCenterId > 0)
                {
                    query = query.Where(i => i.CostCenterId == costCenterId);
                }

                var transactions = await query.ToListAsync();
                return transactions
                    .GroupBy(i => i.CostCenter)
                    .Select(g => new CostCenterReportViewModel
                    {
                        CostCenterName = g.Key?.Name ?? "بدون مركز تكلفة",
                        TotalRevenue = g.Where(i => i.Account.AccountType == AccountType.Revenue).Sum(i => i.Credit - i.Debit),
                        TotalExpenses = g.Where(i => i.Account.AccountType == AccountType.Expense).Sum(i => i.Debit - i.Credit)
                    })
                    .ToList();
            }
        }

        public async Task<List<Budget>> GetBudgetsForFilterAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Budgets.Where(b => b.IsActive).OrderByDescending(b => b.Year).ToListAsync();
            }
        }

        public async Task<List<BudgetVsActualViewModel>> GetBudgetVsActualReportAsync(int budgetId, int month, int year)
        {
            using (var db = new DatabaseContext())
            {
                var budgetDetails = await db.BudgetDetails
                    .Include(bd => bd.Account)
                    .Where(bd => bd.BudgetId == budgetId)
                    .ToListAsync();

                var reportData = new List<BudgetVsActualViewModel>();
                DateTime fromDate = new DateTime(year, month, 1);
                DateTime toDate = fromDate.AddMonths(1).AddDays(-1);

                foreach (var detail in budgetDetails)
                {
                    decimal budgetedAmount = (decimal)detail.GetType().GetProperty($"Month{month}Amount").GetValue(detail, null);
                    decimal actualAmount = await db.JournalVoucherItems
                        .Where(i => i.AccountId == detail.AccountId &&
                                    i.JournalVoucher.VoucherDate >= fromDate &&
                                    i.JournalVoucher.VoucherDate <= toDate)
                        .SumAsync(i => i.Account.AccountType == AccountType.Revenue ? i.Credit - i.Debit : i.Debit - i.Credit);

                    if (budgetedAmount != 0 || actualAmount != 0)
                    {
                        reportData.Add(new BudgetVsActualViewModel
                        {
                            AccountType = detail.Account.AccountType,
                            AccountNumber = detail.Account.AccountNumber,
                            AccountName = detail.Account.AccountName,
                            BudgetedAmount = budgetedAmount,
                            ActualAmount = actualAmount
                        });
                    }
                }
                return reportData.OrderBy(r => r.AccountNumber).ToList();
            }
        }
        #endregion

        #region دوال مساعدة خاصة بالخدمة
        // **تصحيح**: تعديل نوع الإرجاع من ObservableCollection إلى IEnumerable ليكون أكثر مرونة
        private IEnumerable<BalanceSheetAccountViewModel> BuildAccountTree(AccountType type, List<Account> allAccounts, List<JournalVoucherItem> allTransactions, int? parentId = null)
        {
            var childAccounts = allAccounts.Where(a => a.ParentAccountId == parentId && a.AccountType == type).ToList();

            foreach (var account in childAccounts)
            {
                decimal balance = CalculateAccountBalance(account.Id, allTransactions);

                if (type == AccountType.Liability || type == AccountType.Equity)
                {
                    balance = -balance;
                }

                var subAccounts = BuildAccountTree(type, allAccounts, allTransactions, account.Id).ToList();

                var node = new BalanceSheetAccountViewModel
                {
                    AccountName = account.AccountName,
                    Balance = balance + subAccounts.Sum(sa => sa.Balance),
                    SubAccounts = new ObservableCollection<BalanceSheetAccountViewModel>(subAccounts)
                };

                if (node.Balance != 0 || node.SubAccounts.Any())
                {
                    yield return node;
                }
            }
        }

        private decimal CalculateAccountBalance(int accountId, List<JournalVoucherItem> transactions)
        {
            var accountTransactions = transactions.Where(t => t.AccountId == accountId);
            decimal totalDebit = accountTransactions.Sum(t => t.Debit);
            decimal totalCredit = accountTransactions.Sum(t => t.Credit);
            return totalDebit - totalCredit;
        }

        private decimal CalculateNetProfitLoss(DatabaseContext db, DateTime toDate)
        {
            var startOfYear = new DateTime(toDate.Year, 1, 1);

            var revenues = db.JournalVoucherItems.Include(i => i.Account)
                             .Where(i => i.Account.AccountType == AccountType.Revenue && i.JournalVoucher.VoucherDate >= startOfYear && i.JournalVoucher.VoucherDate <= toDate)
                             .Sum(i => i.Credit - i.Debit);

            var expenses = db.JournalVoucherItems.Include(i => i.Account)
                             .Where(i => i.Account.AccountType == AccountType.Expense && i.JournalVoucher.VoucherDate >= startOfYear && i.JournalVoucher.VoucherDate <= toDate)
                             .Sum(i => i.Debit - i.Credit);

            return revenues - expenses;
        }

        private async Task<decimal> CalculateNetProfitLossForPeriodAsync(DatabaseContext db, DateTime fromDate, DateTime toDate)
        {
            var revenues = await db.JournalVoucherItems.Include(i => i.Account)
                             .Where(i => i.Account.AccountType == AccountType.Revenue && i.JournalVoucher.VoucherDate >= fromDate && i.JournalVoucher.VoucherDate <= toDate)
                             .SumAsync(i => i.Credit - i.Debit);

            var expenses = await db.JournalVoucherItems.Include(i => i.Account)
                             .Where(i => i.Account.AccountType == AccountType.Expense && i.JournalVoucher.VoucherDate >= fromDate && i.JournalVoucher.VoucherDate <= toDate)
                             .SumAsync(i => i.Debit - i.Credit);

            return revenues - expenses;
        }

        private async Task<decimal> GetAccountChangeAsync(DatabaseContext db, AccountType type, string accountNameSubstring, DateTime fromDate, DateTime toDate)
        {
            var account = await db.Accounts.FirstOrDefaultAsync(a => a.AccountType == type && a.AccountName.Contains(accountNameSubstring));
            if (account == null) return 0;

            decimal startBalance = await db.JournalVoucherItems
                .Where(i => i.AccountId == account.Id && i.JournalVoucher.VoucherDate < fromDate)
                .SumAsync(i => i.Debit - i.Credit);

            decimal endBalance = await db.JournalVoucherItems
                .Where(i => i.AccountId == account.Id && i.JournalVoucher.VoucherDate <= toDate)
                .SumAsync(i => i.Debit - i.Credit);

            return endBalance - startBalance;
        }
        #endregion
    }
}