// UI/Views/ReportsView.xaml.cs
// *** الكود الكامل للكود الخلفي لشاشة التقارير ***
using GoodMorningFactory.Core.Documents;
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GoodMorningFactory.UI.Views
{
    public partial class ReportsView : UserControl
    {
        private List<Sale> _currentSalesReportData = new List<Sale>();
        private List<Purchase> _currentPurchasesReportData = new List<Purchase>();
        private List<InventoryViewModel> _currentInventoryReportData = new List<InventoryViewModel>();

        public ReportsView()
        {
            InitializeComponent();

            // تعيين التواريخ الافتراضية للفلاتر
            SalesFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            SalesToDatePicker.SelectedDate = DateTime.Now;
            PurchasesFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            PurchasesToDatePicker.SelectedDate = DateTime.Now;
            GlFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            GlToDatePicker.SelectedDate = DateTime.Now;
            TbDatePicker.SelectedDate = DateTime.Now;
            BsDatePicker.SelectedDate = DateTime.Now;
            IsFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, 1, 1);
            IsToDatePicker.SelectedDate = DateTime.Now;
            CfFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            CfToDatePicker.SelectedDate = DateTime.Now;
            WoFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            WoToDatePicker.SelectedDate = DateTime.Now;
            PcFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            PcToDatePicker.SelectedDate = DateTime.Now;
            EffFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EffToDatePicker.SelectedDate = DateTime.Now;
            ScrapFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            ScrapToDatePicker.SelectedDate = DateTime.Now;
            CcFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            CcToDatePicker.SelectedDate = DateTime.Now;

            // تحميل البيانات الأولية للفلاتر والتقارير
            LoadInventoryReport();
            LoadGlAccounts();
            LoadWorkOrderStatusFilter();
            LoadCompletedWorkOrders();
            LoadCostCenterFilter();
            LoadBudgetsFilter(); // ** إضافة جديدة **

        }

        #region Reports Methods (Sales, Purchase, Inventory, GL, TB, IS)
        private void GenerateSalesReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (SalesFromDatePicker.SelectedDate == null || SalesToDatePicker.SelectedDate == null) { return; }
            DateTime fromDate = SalesFromDatePicker.SelectedDate.Value.Date;
            DateTime toDate = SalesToDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);
            try
            {
                using (var db = new DatabaseContext())
                {
                    _currentSalesReportData = db.Sales
                        .Where(s => s.SaleDate >= fromDate && s.SaleDate <= toDate)
                        .OrderByDescending(s => s.SaleDate).ToList();
                    SalesReportDataGrid.ItemsSource = _currentSalesReportData;
                    TotalSalesTextBlock.Text = $"{_currentSalesReportData.Sum(s => s.TotalAmount):N2} {AppSettings.DefaultCurrencySymbol}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportSalesToPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentSalesReportData.Any()) { MessageBox.Show("يرجى توليد تقرير المبيعات أولاً.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "PDF Document (*.pdf)|*.pdf", FileName = $"SalesReport_{DateTime.Now:yyyy-MM-dd}.pdf" };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var companyInfo = db.CompanyInfos.FirstOrDefault();
                        string period = $"الفترة من: {SalesFromDatePicker.SelectedDate:d} إلى: {SalesToDatePicker.SelectedDate:d}";
                        var document = new SalesReportDocument(_currentSalesReportData, companyInfo, period);
                        document.GeneratePdf(saveFileDialog.FileName);
                        MessageBox.Show($"تم تصدير تقرير المبيعات بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل تصدير التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GeneratePurchasesReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (PurchasesFromDatePicker.SelectedDate == null || PurchasesToDatePicker.SelectedDate == null) { return; }
            DateTime fromDate = PurchasesFromDatePicker.SelectedDate.Value.Date;
            DateTime toDate = PurchasesToDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);
            try
            {
                using (var db = new DatabaseContext())
                {
                    _currentPurchasesReportData = db.Purchases.Include(p => p.Supplier)
                        .Where(p => p.PurchaseDate >= fromDate && p.PurchaseDate <= toDate)
                        .OrderByDescending(p => p.PurchaseDate).ToList();
                    PurchasesReportDataGrid.ItemsSource = _currentPurchasesReportData;
                    TotalPurchasesTextBlock.Text = $"{_currentPurchasesReportData.Sum(p => p.TotalAmount):N2} {AppSettings.DefaultCurrencySymbol}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportPurchasesToPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentPurchasesReportData.Any()) { MessageBox.Show("يرجى توليد تقرير المشتريات أولاً.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "PDF Document (*.pdf)|*.pdf", FileName = $"PurchaseReport_{DateTime.Now:yyyy-MM-dd}.pdf" };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var companyInfo = db.CompanyInfos.FirstOrDefault();
                        string period = $"الفترة من: {PurchasesFromDatePicker.SelectedDate:d} إلى: {PurchasesToDatePicker.SelectedDate:d}";
                        var document = new PurchaseReportDocument(_currentPurchasesReportData, companyInfo, period);
                        document.GeneratePdf(saveFileDialog.FileName);
                        MessageBox.Show($"تم تصدير تقرير المشتريات بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex) { MessageBox.Show($"فشل تصدير التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void LoadInventoryReport()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    _currentInventoryReportData = (from p in db.Products.Include(p => p.Category)
                                                   join i in db.Inventories on p.Id equals i.ProductId into gj
                                                   from subInv in gj.DefaultIfEmpty()
                                                   select new InventoryViewModel
                                                   {
                                                       ProductId = p.Id,
                                                       ProductCode = p.ProductCode,
                                                       ProductName = p.Name,
                                                       CategoryName = p.Category.Name ?? "غير مصنف",
                                                       QuantityOnHand = subInv == null ? 0 : subInv.Quantity
                                                   }).ToList();
                    InventoryReportDataGrid.ItemsSource = _currentInventoryReportData;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل تقرير المخزون: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportInventoryToPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentInventoryReportData.Any()) { MessageBox.Show("لا توجد بيانات لتصديرها.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "PDF Document (*.pdf)|*.pdf", FileName = $"InventoryReport_{DateTime.Now:yyyy-MM-dd}.pdf" };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        var companyInfo = db.CompanyInfos.FirstOrDefault();
                        var document = new InventoryReportDocument(_currentInventoryReportData, companyInfo);
                        document.GeneratePdf(saveFileDialog.FileName);
                        MessageBox.Show($"تم تصدير تقرير المخزون بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex) { MessageBox.Show($"فشل تصدير التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }
        private void LoadGlAccounts()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    GlAccountComboBox.ItemsSource = db.Accounts.OrderBy(a => a.AccountNumber).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل الحسابات: {ex.Message}");
            }
        }

        private void GenerateGlReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (GlAccountComboBox.SelectedItem == null || GlFromDatePicker.SelectedDate == null || GlToDatePicker.SelectedDate == null)
            {
                MessageBox.Show("يرجى اختيار الحساب وتحديد الفترة الزمنية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int accountId = (int)GlAccountComboBox.SelectedValue;
            DateTime fromDate = GlFromDatePicker.SelectedDate.Value.Date;
            DateTime toDate = GlToDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                using (var db = new DatabaseContext())
                {
                    decimal openingDebit = db.JournalVoucherItems.Where(i => i.AccountId == accountId && i.JournalVoucher.VoucherDate < fromDate).Sum(i => i.Debit);
                    decimal openingCredit = db.JournalVoucherItems.Where(i => i.AccountId == accountId && i.JournalVoucher.VoucherDate < fromDate).Sum(i => i.Credit);
                    decimal openingBalance = openingDebit - openingCredit;

                    OpeningBalanceRun.Text = $"{openingBalance:N2} {AppSettings.DefaultCurrencySymbol}";

                    var transactions = db.JournalVoucherItems
                        .Include(i => i.JournalVoucher)
                        .Where(i => i.AccountId == accountId && i.JournalVoucher.VoucherDate >= fromDate && i.JournalVoucher.VoucherDate <= toDate)
                        .OrderBy(i => i.JournalVoucher.VoucherDate)
                        .ToList();

                    var reportItems = new List<GeneralLedgerItemViewModel>();
                    decimal currentBalance = openingBalance;

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

                    GlReportDataGrid.ItemsSource = reportItems;

                    decimal totalDebit = transactions.Sum(i => i.Debit);
                    decimal totalCredit = transactions.Sum(i => i.Credit);

                    TotalDebitRun.Text = $"{totalDebit:N2} {AppSettings.DefaultCurrencySymbol}";
                    TotalCreditRun.Text = $"{totalCredit:N2} {AppSettings.DefaultCurrencySymbol}";
                    ClosingBalanceRun.Text = $"{currentBalance:N2} {AppSettings.DefaultCurrencySymbol}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateTbReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (TbDatePicker.SelectedDate == null)
            {
                MessageBox.Show("يرجى تحديد التاريخ لميزان المراجعة.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime toDate = TbDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                using (var db = new DatabaseContext())
                {
                    var reportItems = new List<TrialBalanceItemViewModel>();
                    var accounts = db.Accounts.ToList();

                    decimal grandTotalDebit = 0;
                    decimal grandTotalCredit = 0;

                    foreach (var account in accounts)
                    {
                        var totalDebit = db.JournalVoucherItems
                            .Where(i => i.AccountId == account.Id && i.JournalVoucher.VoucherDate <= toDate)
                            .Sum(i => i.Debit);

                        var totalCredit = db.JournalVoucherItems
                            .Where(i => i.AccountId == account.Id && i.JournalVoucher.VoucherDate <= toDate)
                            .Sum(i => i.Credit);

                        decimal balance = totalDebit - totalCredit;

                        if (balance != 0)
                        {
                            var item = new TrialBalanceItemViewModel
                            {
                                AccountNumber = account.AccountNumber,
                                AccountName = account.AccountName,
                                DebitBalance = balance > 0 ? balance : 0,
                                CreditBalance = balance < 0 ? -balance : 0
                            };
                            reportItems.Add(item);

                            grandTotalDebit += item.DebitBalance;
                            grandTotalCredit += item.CreditBalance;
                        }
                    }

                    TbReportDataGrid.ItemsSource = reportItems;

                    TbTotalDebitRun.Text = $"{grandTotalDebit:N2} {AppSettings.DefaultCurrencySymbol}";
                    TbTotalCreditRun.Text = $"{grandTotalCredit:N2} {AppSettings.DefaultCurrencySymbol}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void GenerateIsReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsFromDatePicker.SelectedDate == null || IsToDatePicker.SelectedDate == null)
            {
                MessageBox.Show("يرجى تحديد الفترة الزمنية لقائمة الدخل.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime fromDate = IsFromDatePicker.SelectedDate.Value.Date;
            DateTime toDate = IsToDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                using (var db = new DatabaseContext())
                {
                    var revenueAccounts = db.Accounts.Where(a => a.AccountType == AccountType.Revenue).ToList();
                    var revenueItems = new List<IncomeStatementItemViewModel>();
                    decimal totalRevenue = 0;
                    foreach (var account in revenueAccounts)
                    {
                        var balance = db.JournalVoucherItems.Where(i => i.AccountId == account.Id && i.JournalVoucher.VoucherDate >= fromDate && i.JournalVoucher.VoucherDate <= toDate).Sum(i => i.Credit - i.Debit);
                        if (balance != 0)
                        {
                            revenueItems.Add(new IncomeStatementItemViewModel { AccountName = account.AccountName, Balance = balance });
                            totalRevenue += balance;
                        }
                    }
                    RevenueDataGrid.ItemsSource = revenueItems;

                    var expenseAccounts = db.Accounts.Where(a => a.AccountType == AccountType.Expense).ToList();
                    var expenseItems = new List<IncomeStatementItemViewModel>();
                    decimal totalExpense = 0;
                    foreach (var account in expenseAccounts)
                    {
                        var balance = db.JournalVoucherItems.Where(i => i.AccountId == account.Id && i.JournalVoucher.VoucherDate >= fromDate && i.JournalVoucher.VoucherDate <= toDate).Sum(i => i.Debit - i.Credit);
                        if (balance != 0)
                        {
                            expenseItems.Add(new IncomeStatementItemViewModel { AccountName = account.AccountName, Balance = balance });
                            totalExpense += balance;
                        }
                    }
                    ExpenseDataGrid.ItemsSource = expenseItems;

                    decimal netProfitLoss = totalRevenue - totalExpense;

                    NetProfitLossRun.Text = $"{netProfitLoss:N2} {AppSettings.DefaultCurrencySymbol}";
                    NetProfitLossRun.Foreground = netProfitLoss >= 0 ? Brushes.Green : Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Balance Sheet Methods
        private void GenerateBsReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (BsDatePicker.SelectedDate == null)
            {
                MessageBox.Show("يرجى تحديد تاريخ الميزانية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime toDate = BsDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                using (var db = new DatabaseContext())
                {
                    var allAccounts = db.Accounts.ToList();
                    var allTransactions = db.JournalVoucherItems.Include(i => i.JournalVoucher)
                                          .Where(i => i.JournalVoucher.VoucherDate <= toDate).ToList();

                    var assets = BuildAccountTree(AccountType.Asset, allAccounts, allTransactions);
                    AssetsTreeView.ItemsSource = assets;
                    decimal totalAssets = assets.Sum(a => a.Balance);

                    var liabilities = BuildAccountTree(AccountType.Liability, allAccounts, allTransactions);
                    LiabilitiesTreeView.ItemsSource = liabilities;
                    decimal totalLiabilities = liabilities.Sum(l => l.Balance);

                    var equity = BuildAccountTree(AccountType.Equity, allAccounts, allTransactions);
                    decimal netProfitLoss = CalculateNetProfitLoss(db, toDate);
                    equity.Add(new BalanceSheetAccountViewModel { AccountName = "صافي الربح (الخسارة) للفترة", Balance = netProfitLoss });
                    EquityTreeView.ItemsSource = equity;
                    decimal totalEquity = equity.Sum(eq => eq.Balance);

                    TotalAssetsRun.Text = $"{totalAssets:N2} {AppSettings.DefaultCurrencySymbol}";
                    TotalLiabilitiesAndEquityRun.Text = $"{(totalLiabilities + totalEquity):N2} {AppSettings.DefaultCurrencySymbol}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ObservableCollection<BalanceSheetAccountViewModel> BuildAccountTree(AccountType type, List<Account> allAccounts, List<JournalVoucherItem> allTransactions, int? parentId = null)
        {
            var nodes = new ObservableCollection<BalanceSheetAccountViewModel>();
            var childAccounts = allAccounts.Where(a => a.ParentAccountId == parentId && a.AccountType == type).ToList();

            foreach (var account in childAccounts)
            {
                decimal balance = CalculateAccountBalance(account.Id, allTransactions);

                if (type == AccountType.Liability || type == AccountType.Equity)
                {
                    balance = -balance;
                }

                var node = new BalanceSheetAccountViewModel
                {
                    AccountName = account.AccountName,
                    Balance = balance,
                    SubAccounts = BuildAccountTree(type, allAccounts, allTransactions, account.Id)
                };

                node.Balance += node.SubAccounts.Sum(sa => sa.Balance);

                if (node.Balance != 0 || node.SubAccounts.Any())
                {
                    nodes.Add(node);
                }
            }
            return nodes;
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
        #endregion

        #region Cash Flow Methods
        private void GenerateCashFlowReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (CfFromDatePicker.SelectedDate == null || CfToDatePicker.SelectedDate == null)
            {
                MessageBox.Show("يرجى تحديد الفترة الزمنية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime fromDate = CfFromDatePicker.SelectedDate.Value.Date;
            DateTime toDate = CfToDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                using (var db = new DatabaseContext())
                {
                    var reportItems = new ObservableCollection<CashFlowItemViewModel>();

                    reportItems.Add(new CashFlowItemViewModel { Item = "التدفقات النقدية من الأنشطة التشغيلية", IsTotal = true, IndentLevel = 0 });
                    decimal netIncome = CalculateNetProfitLossForPeriod(db, fromDate, toDate);
                    reportItems.Add(new CashFlowItemViewModel { Item = "صافي الدخل", Amount = netIncome, IndentLevel = 1 });
                    reportItems.Add(new CashFlowItemViewModel { Item = "تسويات لتحويل صافي الدخل إلى نقدية:", IndentLevel = 1 });
                    decimal arChange = GetAccountChange(db, AccountType.Asset, "الذمم المدينة", fromDate, toDate);
                    reportItems.Add(new CashFlowItemViewModel { Item = "النقص (الزيادة) في الذمم المدينة", Amount = -arChange, IndentLevel = 2 });
                    decimal inventoryChange = GetAccountChange(db, AccountType.Asset, "المخزون", fromDate, toDate);
                    reportItems.Add(new CashFlowItemViewModel { Item = "النقص (الزيادة) في المخزون", Amount = -inventoryChange, IndentLevel = 2 });
                    decimal apChange = GetAccountChange(db, AccountType.Liability, "الذمم الدائنة", fromDate, toDate);
                    reportItems.Add(new CashFlowItemViewModel { Item = "الزيادة (النقص) في الذمم الدائنة", Amount = apChange, IndentLevel = 2 });
                    decimal totalOperatingCashFlow = netIncome - arChange - inventoryChange + apChange;
                    reportItems.Add(new CashFlowItemViewModel { Item = "صافي النقدية من الأنشطة التشغيلية", Amount = totalOperatingCashFlow, IndentLevel = 0, IsTotal = true });

                    CashFlowDataGrid.ItemsSource = reportItems;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private decimal CalculateNetProfitLossForPeriod(DatabaseContext db, DateTime fromDate, DateTime toDate)
        {
            var revenues = db.JournalVoucherItems.Include(i => i.Account)
                             .Where(i => i.Account.AccountType == AccountType.Revenue && i.JournalVoucher.VoucherDate >= fromDate && i.JournalVoucher.VoucherDate <= toDate)
                             .Sum(i => i.Credit - i.Debit);

            var expenses = db.JournalVoucherItems.Include(i => i.Account)
                             .Where(i => i.Account.AccountType == AccountType.Expense && i.JournalVoucher.VoucherDate >= fromDate && i.JournalVoucher.VoucherDate <= toDate)
                             .Sum(i => i.Debit - i.Credit);

            return revenues - expenses;
        }

        private decimal GetAccountChange(DatabaseContext db, AccountType type, string accountNameSubstring, DateTime fromDate, DateTime toDate)
        {
            var account = db.Accounts.FirstOrDefault(a => a.AccountType == type && a.AccountName.Contains(accountNameSubstring));
            if (account == null) return 0;

            decimal startBalance = db.JournalVoucherItems
                .Where(i => i.AccountId == account.Id && i.JournalVoucher.VoucherDate < fromDate)
                .Sum(i => i.Debit - i.Credit);

            decimal endBalance = db.JournalVoucherItems
                .Where(i => i.AccountId == account.Id && i.JournalVoucher.VoucherDate <= toDate)
                .Sum(i => i.Debit - i.Credit);

            return endBalance - startBalance;
        }
        #endregion

        #region Production Reports Methods
        private void LoadWorkOrderStatusFilter()
        {
            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(WorkOrderStatus)).Cast<object>());
            WoStatusFilterComboBox.ItemsSource = statuses;
            WoStatusFilterComboBox.SelectedIndex = 0;
        }

        private void GenerateWoReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (WoFromDatePicker.SelectedDate == null || WoToDatePicker.SelectedDate == null) return;
            DateTime fromDate = WoFromDatePicker.SelectedDate.Value.Date;
            DateTime toDate = WoToDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.WorkOrders.Include(wo => wo.FinishedGood).AsQueryable();
                    if (WoStatusFilterComboBox.SelectedItem != null && WoStatusFilterComboBox.SelectedItem is WorkOrderStatus status)
                    {
                        query = query.Where(wo => wo.Status == status);
                    }
                    query = query.Where(wo => wo.PlannedStartDate >= fromDate && wo.PlannedStartDate <= toDate);
                    WoReportDataGrid.ItemsSource = query.OrderByDescending(wo => wo.PlannedStartDate).ToList();
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
        }

        private void GeneratePcReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (PcFromDatePicker.SelectedDate == null || PcToDatePicker.SelectedDate == null) return;
            DateTime fromDate = PcFromDatePicker.SelectedDate.Value.Date;
            DateTime toDate = PcToDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                using (var db = new DatabaseContext())
                {
                    var reportItems = new List<ProductionCostReportViewModel>();
                    var completedWorkOrders = db.WorkOrders
                        .Include(wo => wo.FinishedGood)
                        .Where(wo => wo.Status == WorkOrderStatus.Completed &&
                                     wo.ActualEndDate >= fromDate && wo.ActualEndDate <= toDate)
                        .ToList();

                    foreach (var wo in completedWorkOrders)
                    {
                        var consumedMaterials = db.WorkOrderMaterials
                            .Include(m => m.RawMaterial)
                            .Where(m => m.WorkOrderId == wo.Id)
                            .ToList();
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
                    PcReportDataGrid.ItemsSource = reportItems;
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
        }

        private void LoadCompletedWorkOrders()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var completedOrders = db.WorkOrders
                        .Where(wo => wo.Status == WorkOrderStatus.Completed)
                        .OrderByDescending(wo => wo.WorkOrderNumber)
                        .ToList();
                    McWorkOrderComboBox.ItemsSource = completedOrders;
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل أوامر العمل: {ex.Message}"); }
        }

        private void McWorkOrderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (McWorkOrderComboBox.SelectedItem == null) { McReportDataGrid.ItemsSource = null; return; }
            int workOrderId = (int)McWorkOrderComboBox.SelectedValue;

            try
            {
                using (var db = new DatabaseContext())
                {
                    var reportItems = new List<MaterialConsumptionReportViewModel>();
                    var workOrder = db.WorkOrders.Find(workOrderId);
                    if (workOrder == null) return;

                    var bom = db.BillOfMaterials.Include(b => b.BillOfMaterialsItems).ThenInclude(i => i.RawMaterial)
                                .FirstOrDefault(b => b.FinishedGoodId == workOrder.FinishedGoodId);
                    if (bom == null) { McReportDataGrid.ItemsSource = null; return; }

                    var consumedMaterials = db.WorkOrderMaterials
                        .Where(m => m.WorkOrderId == workOrderId)
                        .ToList();

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
                    McReportDataGrid.ItemsSource = reportItems;
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
        }

        private void GenerateEffReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (EffFromDatePicker.SelectedDate == null || EffToDatePicker.SelectedDate == null) return;
            DateTime fromDate = EffFromDatePicker.SelectedDate.Value.Date;
            DateTime toDate = EffToDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                using (var db = new DatabaseContext())
                {
                    var reportItems = new List<ProductionEfficiencyReportViewModel>();
                    var completedWorkOrders = db.WorkOrders
                        .Include(wo => wo.FinishedGood)
                        .Where(wo => wo.Status == WorkOrderStatus.Completed &&
                                     wo.ActualStartDate.HasValue &&
                                     wo.ActualEndDate.HasValue &&
                                     wo.ActualEndDate >= fromDate && wo.ActualEndDate <= toDate)
                        .ToList();

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
                    EffReportDataGrid.ItemsSource = reportItems;
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
        }

        private void GenerateScrapReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScrapFromDatePicker.SelectedDate == null || ScrapToDatePicker.SelectedDate == null) return;
            DateTime fromDate = ScrapFromDatePicker.SelectedDate.Value.Date;
            DateTime toDate = ScrapToDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                using (var db = new DatabaseContext())
                {
                    var scrapReport = db.WorkOrderScraps
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
                        .ToList();
                    ScrapReportDataGrid.ItemsSource = scrapReport;
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
        }
        #endregion

        #region Cost Center Report Methods
        private void LoadCostCenterFilter()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var costCenters = new List<object> { new { Name = "الكل", Id = 0 } };
                    costCenters.AddRange(db.CostCenters.Where(c => c.IsActive).ToList());
                    CcFilterComboBox.ItemsSource = costCenters;
                    CcFilterComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل مراكز التكلفة: {ex.Message}"); }
        }

        private void GenerateCcReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (CcFromDatePicker.SelectedDate == null || CcToDatePicker.SelectedDate == null) return;
            DateTime fromDate = CcFromDatePicker.SelectedDate.Value.Date;
            DateTime toDate = CcToDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);
            int selectedCostCenterId = (int?)CcFilterComboBox.SelectedValue ?? 0;

            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.JournalVoucherItems
                                  .Include(i => i.Account)
                                  .Include(i => i.CostCenter)
                                  .Where(i => i.JournalVoucher.VoucherDate >= fromDate && i.JournalVoucher.VoucherDate <= toDate)
                                  .Where(i => i.Account.AccountType == AccountType.Revenue || i.Account.AccountType == AccountType.Expense);

                    if (selectedCostCenterId > 0)
                    {
                        query = query.Where(i => i.CostCenterId == selectedCostCenterId);
                    }

                    var transactions = query.ToList();
                    var reportData = transactions
                        .GroupBy(i => i.CostCenter)
                        .Select(g => new CostCenterReportViewModel
                        {
                            CostCenterName = g.Key?.Name ?? "بدون مركز تكلفة",
                            TotalRevenue = g.Where(i => i.Account.AccountType == AccountType.Revenue).Sum(i => i.Credit - i.Debit),
                            TotalExpenses = g.Where(i => i.Account.AccountType == AccountType.Expense).Sum(i => i.Debit - i.Credit)
                        })
                        .ToList();

                    CcReportDataGrid.ItemsSource = reportData;

                    decimal grandTotalRevenue = reportData.Sum(r => r.TotalRevenue);
                    decimal grandTotalExpense = reportData.Sum(r => r.TotalExpenses);
                    decimal grandTotalNet = grandTotalRevenue - grandTotalExpense;

                    CcTotalRevenueRun.Text = $"{grandTotalRevenue:N2} {AppSettings.DefaultCurrencySymbol}";
                    CcTotalExpenseRun.Text = $"{grandTotalExpense:N2} {AppSettings.DefaultCurrencySymbol}";
                    CcTotalNetRun.Text = $"{grandTotalNet:N2} {AppSettings.DefaultCurrencySymbol}";
                    CcTotalNetRun.Foreground = grandTotalNet >= 0 ? Brushes.Green : Brushes.Red;
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
        }



        // *** بداية الإضافة: منطقة تقرير الموازنة مقابل الفعلي ***

        private void LoadBudgetsFilter()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var budgets = db.Budgets.Where(b => b.IsActive).OrderByDescending(b => b.Year).ToList();
                    BudgetFilterComboBox.ItemsSource = budgets;
                    if (budgets.Any())
                    {
                        BudgetFilterComboBox.SelectedIndex = 0;
                    }

                    // ملء قائمة الشهور
                    BudgetMonthFilterComboBox.ItemsSource = Enumerable.Range(1, 12).Select(m => new { Month = m, Name = new DateTime(2000, m, 1).ToString("MMMM", new CultureInfo("ar-KW")) });
                    BudgetMonthFilterComboBox.DisplayMemberPath = "Name";
                    BudgetMonthFilterComboBox.SelectedValuePath = "Month";
                    BudgetMonthFilterComboBox.SelectedValue = DateTime.Now.Month;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل قائمة الموازنات: {ex.Message}");
            }
        }

        private void GenerateBudgetVsActualReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (BudgetFilterComboBox.SelectedItem == null || BudgetMonthFilterComboBox.SelectedItem == null)
            {
                MessageBox.Show("يرجى اختيار الموازنة والشهر لعرض التقرير.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int budgetId = (int)BudgetFilterComboBox.SelectedValue;
            int selectedMonth = (int)BudgetMonthFilterComboBox.SelectedValue;
            int selectedYear = ((Budget)BudgetFilterComboBox.SelectedItem).Year;

            try
            {
                using (var db = new DatabaseContext())
                {
                    var budgetDetails = db.BudgetDetails
                        .Include(bd => bd.Account)
                        .Where(bd => bd.BudgetId == budgetId)
                        .ToList();

                    if (!budgetDetails.Any())
                    {
                        MessageBox.Show("هذه الموازنة لا تحتوي على تفاصيل.", "معلومة");
                        BudgetVsActualDataGrid.ItemsSource = null;
                        return;
                    }

                    var reportData = new List<BudgetVsActualViewModel>();

                    DateTime fromDate = new DateTime(selectedYear, selectedMonth, 1);
                    DateTime toDate = fromDate.AddMonths(1).AddDays(-1);

                    foreach (var detail in budgetDetails)
                    {
                        // جلب المبلغ التقديري للشهر المحدد
                        decimal budgetedAmount = (decimal)detail.GetType().GetProperty($"Month{selectedMonth}Amount").GetValue(detail, null);

                        // جلب المبلغ الفعلي من القيود
                        decimal actualAmount = db.JournalVoucherItems
                            .Where(i => i.AccountId == detail.AccountId &&
                                        i.JournalVoucher.VoucherDate >= fromDate &&
                                        i.JournalVoucher.VoucherDate <= toDate)
                            .Sum(i => i.Account.AccountType == AccountType.Revenue ? i.Credit - i.Debit : i.Debit - i.Credit);

                        // إضافة السطر للتقرير فقط إذا كان هناك مبلغ تقديري أو فعلي
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

                    BudgetVsActualDataGrid.ItemsSource = reportData.OrderBy(r => r.AccountNumber);

                    // حساب الإجماليات
                    decimal totalBudget = reportData.Sum(r => r.BudgetedAmount);
                    decimal totalActual = reportData.Sum(r => r.ActualAmount);
                    decimal totalVariance = totalActual - totalBudget;

                    BudgetTotalRun.Text = $"{totalBudget:N2} {AppSettings.DefaultCurrencySymbol}";
                    ActualTotalRun.Text = $"{totalActual:N2} {AppSettings.DefaultCurrencySymbol}";
                    VarianceTotalRun.Text = $"{totalVariance:N2} {AppSettings.DefaultCurrencySymbol}";
                    VarianceTotalRun.Foreground = totalVariance >= 0 ? Brushes.Red : Brushes.Green; // نفترض أن الزيادة انحراف غير جيد بشكل عام
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ");
            }
        }

        // *** نهاية الإضافة ***
        #endregion
    }
}
