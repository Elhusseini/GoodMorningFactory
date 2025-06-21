// UI/Views/MainWindow.xaml.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.UI.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ApplyPermissions(); // تطبيق الصلاحيات التي تم تحميلها مسبقاً
            if (CurrentUserService.LoggedInUser != null)
            {
                CurrentUserTextBlock.Text = $"المستخدم: {CurrentUserService.LoggedInUser.Username}";
            }
            else
            {
                MessageBox.Show("فشل تحميل بيانات المستخدم. سيتم إغلاق البرنامج.");
                Application.Current.Shutdown();
                return;
            }
            MainContentArea.Content = new DashboardView();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CheckForLowStockNotifications();
        }

        private void CheckForLowStockNotifications()
        {
            if (!PermissionsService.CanAccess("Inventory.LowStock.View")) return;
            var notificationView = new LowStockNotificationsView();
            if (notificationView.HasLowStockItems)
            {
                var notificationWindow = new Window
                {
                    Title = "تنبيه انخفاض المخزون",
                    Content = notificationView,
                    Width = 800,
                    Height = 450,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    FlowDirection = FlowDirection.RightToLeft
                };
                notificationWindow.ShowDialog();
            }
        }

        private void LoadCurrentUserAndApplyPermissions()
        {
            if (CurrentUserService.LoggedInUser == null)
            {
                MessageBox.Show("خطأ حرج: لم يتم العثور على بيانات المستخدم. سيتم إغلاق البرنامج.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
            try
            {
                using (var db = new DatabaseContext())
                {
                    CurrentUserService.LoggedInUser = db.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == CurrentUserService.LoggedInUser.Id);
                    if (CurrentUserService.LoggedInUser != null)
                    {
                        PermissionsService.LoadUserPermissions(CurrentUserService.LoggedInUser.RoleId);
                    }
                }
                ApplyPermissions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل في تحميل صلاحيات المستخدم: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        public void NavigateTo(string viewName)
        {
            switch (viewName)
            {
                // لوحات المعلومات
                case "Dashboard": MainContentArea.Content = new DashboardView(); break;
                case "SalesDashboard": MainContentArea.Content = new SalesDashboardView(); break;
                case "ProductionDashboard": MainContentArea.Content = new ProductionDashboardView(); break;
                case "InventoryDashboard": MainContentArea.Content = new InventoryDashboardView(); break;
                case "HRDashboard": MainContentArea.Content = new HRDashboardView(); break;
                case "FinancialDashboard": MainContentArea.Content = new FinancialDashboardView(); break;

                // البيانات الرئيسية
                case "Products": MainContentArea.Content = new ProductsView(); break;
                case "Categories": MainContentArea.Content = new CategoriesView(); break;
                case "UnitsOfMeasure": MainContentArea.Content = new UnitsOfMeasureView(); break;
                case "PriceLists": MainContentArea.Content = new PriceListsView(); break;
                case "Suppliers": MainContentArea.Content = new SuppliersView(); break;
                case "Customers": MainContentArea.Content = new CustomersView(); break;
                case "Warehouses": MainContentArea.Content = new WarehousesView(); break;
                case "Currencies": MainContentArea.Content = new CurrenciesView(); break;

                // المبيعات
                case "Quotations": MainContentArea.Content = new SalesQuotationsView(); break;
                case "Orders": MainContentArea.Content = new SalesOrdersView(); break;
                case "Shipments": MainContentArea.Content = new ShipmentsView(); break;
                case "Invoices": MainContentArea.Content = new SalesView(); break;
                case "Returns": MainContentArea.Content = new SalesReturnsView(); break;

                // المشتريات
                case "PurchaseRequisitions": MainContentArea.Content = new PurchaseRequisitionsView(); break;
                case "PurchaseOrders": MainContentArea.Content = new PurchaseOrdersView(); break;
                case "GoodsReceipt": MainContentArea.Content = new GoodsReceiptView(); break;
                case "Purchases": MainContentArea.Content = new PurchasesView(); break;

                // التصنيع
                case "BOM": MainContentArea.Content = new BillOfMaterialsView(); break;
                case "WorkOrders": MainContentArea.Content = new WorkOrdersView(); break;
                case "MRP": MainContentArea.Content = new MRPView(); break;

                // المخزون
                case "Inventory": MainContentArea.Content = new InventoryView(); break;
                case "StockMovements": MainContentArea.Content = new StockMovementsView(); break;
                case "StockTransfers": MainContentArea.Content = new StockTransfersView(); break;
                case "InventoryCounts": MainContentArea.Content = new InventoryCountsView(); break;
                case "SerialNumbers": MainContentArea.Content = new SerialNumbersView(); break;
                case "LowStockNotifications": MainContentArea.Content = new LowStockNotificationsView(); break;

                // الموارد البشرية
                case "Employees": MainContentArea.Content = new EmployeesView(); break;
                case "Attendance": MainContentArea.Content = new AttendanceView(); break;
                case "LeaveManagement": MainContentArea.Content = new LeaveManagementView(); break;
                case "Payroll": MainContentArea.Content = new PayrollView(); break;
                case "LeaveTypes": MainContentArea.Content = new LeaveTypesView(); break;

                // الحسابات
                case "ChartOfAccounts": MainContentArea.Content = new ChartOfAccountsView(); break;
                case "JournalVouchers": MainContentArea.Content = new JournalVouchersView(); break;
                case "AccountsReceivable": MainContentArea.Content = new AccountsReceivableView(); break;
                case "AccountsPayable": MainContentArea.Content = new AccountsPayableAgingView(); break;
                case "BankReconciliation": MainContentArea.Content = new BankReconciliationView(); break;
                case "AccountingPeriods": MainContentArea.Content = new AccountingPeriodsView(); break;
                case "FixedAssets": MainContentArea.Content = new FixedAssetsView(); break;
                case "CostCenters": MainContentArea.Content = new CostCentersView(); break;
                case "Budgets": MainContentArea.Content = new BudgetsView(); break;

                // التقارير والإعدادات والأمان
                case "Reports": MainContentArea.Content = new ReportsView(); break;
                case "Settings": MainContentArea.Content = new SettingsView(); break;
                case "Users": MainContentArea.Content = new UsersView(); break;
                case "Roles": MainContentArea.Content = new RolesView(); break;
                case "Departments": MainContentArea.Content = new DepartmentsView(); break;
                case "AuditTrail": MainContentArea.Content = new AuditTrailView(); break;
                case "ApprovalWorkflows": MainContentArea.Content = new ApprovalWorkflowsView(); break;
                case "MyApprovals": MainContentArea.Content = new MyApprovalsView(); break;
                case "CRM": MainContentArea.Content = new CrmView(); break;
            }
        }

        private void ApplyPermissions()
        {
            MainDataMenuItem.Visibility = PermissionsService.CanAccess("MainData.View") ? Visibility.Visible : Visibility.Collapsed;
            CrmMenuItem.Visibility = PermissionsService.CanAccess("CRM.View") ? Visibility.Visible : Visibility.Collapsed;
            SalesMenuItem.Visibility = PermissionsService.CanAccess("Sales.View") ? Visibility.Visible : Visibility.Collapsed;
            PurchasingMenuItem.Visibility = PermissionsService.CanAccess("Purchases.View") ? Visibility.Visible : Visibility.Collapsed;
            ManufacturingMenuItem.Visibility = PermissionsService.CanAccess("Manufacturing.View") ? Visibility.Visible : Visibility.Collapsed;
            InventoryMenuItem.Visibility = PermissionsService.CanAccess("Inventory.View") ? Visibility.Visible : Visibility.Collapsed;
            HrMenuItem.Visibility = PermissionsService.CanAccess("HR.View") ? Visibility.Visible : Visibility.Collapsed;
            FinancialsMenuItem.Visibility = PermissionsService.CanAccess("Financials.View") ? Visibility.Visible : Visibility.Collapsed;
            ReportsMenuItem.Visibility = PermissionsService.CanAccess("Reports.View") ? Visibility.Visible : Visibility.Collapsed;
            SettingsMenuItem.Visibility = PermissionsService.CanAccess("Settings.View") ? Visibility.Visible : Visibility.Collapsed;
            SecurityMenuItem.Visibility = PermissionsService.CanAccess("Security.View") ? Visibility.Visible : Visibility.Collapsed;

            // تطبيق الصلاحيات على القوائم الفرعية
            LowStockMenuItem.Visibility = PermissionsService.CanAccess("Inventory.LowStock.View") ? Visibility.Visible : Visibility.Collapsed;
            AuditTrailMenuItem.Visibility = PermissionsService.CanAccess("Admin.AuditTrail.View") ? Visibility.Visible : Visibility.Collapsed;
            ApprovalWorkflowsMenuItem.Visibility = PermissionsService.CanAccess("Admin.ApprovalWorkflows.Manage") ? Visibility.Visible : Visibility.Collapsed;
            InventoryCountsMenuItem.Visibility = PermissionsService.CanAccess("Inventory.Counts.View") ? Visibility.Visible : Visibility.Collapsed;

            // صلاحية قائمة "موافقاتي" ستكون متاحة للجميع، ولكن الشاشة نفسها ستكون فارغة إذا لم يكن لديهم موافقات
            MyApprovalsMenuItem.Visibility = Visibility.Visible;
        }

        protected override void OnClosing(CancelEventArgs e) { base.OnClosing(e); Application.Current.Shutdown(); }

        // --- دوال التنقل ---
        private void DashboardMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Dashboard");
        private void LogoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            var result = loginWindow.ShowDialog();
            if (result == true)
            {
                LoadCurrentUserAndApplyPermissions();
                CurrentUserTextBlock.Text = $"المستخدم: {CurrentUserService.LoggedInUser.Username}";
                MainContentArea.Content = new DashboardView();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void HelpMenuItem_Click(object sender, RoutedEventArgs e) => new AboutWindow().ShowDialog();

        // البيانات الرئيسية
        private void ManageProductsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Products");
        private void ManageCategoriesMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Categories");
        private void ManageUomMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("UnitsOfMeasure");
        private void ManagePriceListsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("PriceLists");
        private void ManageSuppliersMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Suppliers");
        private void ManageCustomersMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Customers");
        private void ManageWarehousesMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Warehouses");

        // إدارة علاقات العملاء
        private void ManageCrmMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("CRM");
        private void MyApprovalsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("MyApprovals");

        // المبيعات
        private void SalesDashboardMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("SalesDashboard");
        private void ManageQuotationsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Quotations");
        private void ManageOrdersMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Orders");
        private void ManageShipmentsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Shipments");
        private void ManageInvoicesMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Invoices");
        private void ManageSalesReturnsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Returns");

        // المشتريات
        private void PurchaseRequisitionsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("PurchaseRequisitions");
        private void PurchaseOrdersMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("PurchaseOrders");
        private void GoodsReceiptMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("GoodsReceipt");
        private void PurchasesMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Purchases");

        // التصنيع
        private void ProductionDashboardMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("ProductionDashboard");
        private void BomMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("BOM");
        private void WorkOrdersMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("WorkOrders");
        private void MRPMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("MRP");

        // المخزون
        private void InventoryDashboardMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("InventoryDashboard");
        private void ManageInventoryMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Inventory");
        private void StockMovementsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("StockMovements");
        private void StockTransfersMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("StockTransfers");
        private void InventoryCountsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("InventoryCounts");
        private void SerialNumbersMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("SerialNumbers");
        private void LowStockNotificationsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("LowStockNotifications");

        // الموارد البشرية
        private void HRDashboardMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("HRDashboard");
        private void ManageEmployeesMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Employees");
        private void ManageAttendanceMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Attendance");
        private void ManageLeavesMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("LeaveManagement");
        private void LeaveTypesMenuItem_Click(Object sender, RoutedEventArgs e) => NavigateTo("LeaveTypes");
        private void PayrollMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Payroll");

        // الحسابات
        private void FinancialDashboardMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("FinancialDashboard");
        private void ChartOfAccountsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("ChartOfAccounts");
        private void JournalVouchersMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("JournalVouchers");
        private void AccountsReceivableMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("AccountsReceivable");
        private void AccountsPayableMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("AccountsPayable");
        private void BankReconciliationMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("BankReconciliation");
        private void AccountingPeriodsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("AccountingPeriods");
        private void FixedAssetsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("FixedAssets");
        private void CostCentersMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("CostCenters");
        private void BudgetsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Budgets");

        // التقارير
        private void ReportsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Reports");

        // الإعدادات والأمان
        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Settings");
        private void ManageUsersMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Users");
        private void ManageRolesMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("Roles");
        private void DepartmentsButton_Click(object sender, RoutedEventArgs e) => NavigateTo("Departments");
        private void AuditTrailMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("AuditTrail");
        private void ApprovalWorkflowsMenuItem_Click(object sender, RoutedEventArgs e) => NavigateTo("ApprovalWorkflows");
    }
}
