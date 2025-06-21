using GoodMorningFactory.UI.Views;
using System;
using System.Windows.Controls;

namespace GoodMorningFactory.Core.Services
{
    public class NavigationService : INavigationService
    {
        public event Action<UserControl> NavigationRequested;

        public void NavigateTo(string viewName)
        {
            UserControl view = GetViewByName(viewName);
            if (view != null)
            {
                // إطلاق الحدث وإرسال الواجهة الجديدة التي يجب عرضها
                NavigationRequested?.Invoke(view);
            }
        }

        // هذه هي دالة switch الكاملة والشاملة بناءً على ملفات مشروعك
        private UserControl GetViewByName(string viewName)
        {
            switch (viewName)
            {
                // لوحات المعلومات
                case "Dashboard": return new DashboardView();
                case "SalesDashboard": return new SalesDashboardView();
                case "ProductionDashboard": return new ProductionDashboardView();
                case "InventoryDashboard": return new InventoryDashboardView();
                case "HRDashboard": return new HRDashboardView();
                case "FinancialDashboard": return new FinancialDashboardView();
                case "PurchasingDashboard": return new PurchasingDashboardView();

                // البيانات الرئيسية
                case "Products": return new ProductsView();
                case "Categories": return new CategoriesView();
                case "UnitsOfMeasure": return new UnitsOfMeasureView();
                case "PriceLists": return new PriceListsView();
                case "Suppliers": return new SuppliersView();
                case "Customers": return new CustomersView();
                case "Warehouses": return new WarehousesView();
                case "Currencies": return new CurrenciesView();

                // المبيعات
                case "Quotations": return new SalesQuotationsView();
                case "Orders": return new SalesOrdersView();
                case "Shipments": return new ShipmentsView();
                case "Invoices": return new SalesView();
                case "Returns": return new SalesReturnsView();

                // المشتريات
                case "PurchaseRequisitions": return new PurchaseRequisitionsView();
                case "PurchaseOrders": return new PurchaseOrdersView();
                case "GoodsReceipt": return new GoodsReceiptView();
                case "Purchases": return new PurchasesView();
                case "PurchaseReturns": return new PurchaseReturnsView();

                // التصنيع
                case "BOM": return new BillOfMaterialsView();
                case "WorkOrders": return new WorkOrdersView();
                case "ProductionScheduling": return new ProductionSchedulingView();
                case "MRP": return new MRPView();

                // المخزون
                case "Inventory": return new InventoryView();
                case "StockMovements": return new StockMovementsView();
                case "StockTransfers": return new StockTransfersView();
                case "InventoryCounts": return new InventoryCountsView();
                case "SerialNumbers": return new SerialNumbersView();
                case "LowStockNotifications": return new LowStockNotificationsView();

                // الموارد البشرية
                case "Employees": return new EmployeesView();
                case "Attendance": return new AttendanceView();
                case "LeaveManagement": return new LeaveManagementView();
                case "Payroll": return new PayrollView();
                case "LeaveTypes": return new LeaveTypesView();

                // الجودة
                case "QualityParameters": return new QualityParametersView();
                case "QualityChecks": return new QualityChecksView();

                // الحسابات
                case "ChartOfAccounts": return new ChartOfAccountsView();
                case "JournalVouchers": return new JournalVouchersView();
                case "AccountsReceivable": return new AccountsReceivableView();
                case "AccountsPayable": return new AccountsPayableAgingView();
                case "BankReconciliation": return new BankReconciliationView();
                case "AccountingPeriods": return new AccountingPeriodsView();
                case "FixedAssets": return new FixedAssetsView();
                case "CostCenters": return new CostCentersView();
                case "Budgets": return new BudgetsView();

                // التقارير والإعدادات والأمان
                case "Reports": return new ReportsView();
                case "Settings": return new SettingsView();
                case "Users": return new UsersView();
                case "Roles": return new RolesView();
                case "Departments": return new DepartmentsView();
                case "AuditTrail": return new AuditTrailView();
                case "ApprovalWorkflows": return new ApprovalWorkflowsView();
                case "MyApprovals": return new MyApprovalsView();
                case "CRM": return new CrmView();

                // واجهة افتراضية في حال لم يتم العثور على الاسم
                default: return new DashboardView();
            }
        }
    }
}