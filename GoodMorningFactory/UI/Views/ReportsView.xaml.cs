// UI/Views/ReportsView.xaml.cs
// *** الكود الكامل لواجهة التقارير مع إضافة منطق تقرير الهدر ***
using GoodMorningFactory.Core.Documents;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GoodMorningFactory.UI.Views
{
    public partial class ReportsView : UserControl
    {
        // متغيرات لتخزين بيانات التقارير الحالية لغرض التصدير
        private List<Sale> _currentSalesReportData = new List<Sale>();
        private List<Purchase> _currentPurchasesReportData = new List<Purchase>();
        private List<InventoryViewModel> _currentInventoryReportData = new List<InventoryViewModel>();
        public ReportsView()
        {
            InitializeComponent();
            LoadGlAccounts();

            // --- الإعدادات الافتراضية للتواريخ ---
            SalesFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            SalesToDatePicker.SelectedDate = DateTime.Now;
            PurchasesFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            PurchasesToDatePicker.SelectedDate = DateTime.Now;
            GlFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            GlToDatePicker.SelectedDate = DateTime.Now;
            TbDatePicker.SelectedDate = DateTime.Now;
            IsFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            IsToDatePicker.SelectedDate = DateTime.Now;
            WoFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            WoToDatePicker.SelectedDate = DateTime.Now;
            PcFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            PcToDatePicker.SelectedDate = DateTime.Now;
            EffFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EffToDatePicker.SelectedDate = DateTime.Now;
            ScrapFromDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // <-- إضافة جديدة
            ScrapToDatePicker.SelectedDate = DateTime.Now; // <-- إضافة جديدة

            // --- تحميل البيانات الأولية ---
            LoadInventoryReport();
            LoadGlAccounts();
            LoadWorkOrderStatusFilter();
            LoadCompletedWorkOrders();
        }

        // --- منطقة تقارير المبيعات ---
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
                    TotalSalesTextBlock.Text = $"{_currentSalesReportData.Sum(s => s.TotalAmount):C}";
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

        // --- منطقة تقارير المشتريات ---
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
                    TotalPurchasesTextBlock.Text = $"{_currentPurchasesReportData.Sum(p => p.TotalAmount):C}";
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

        // --- منطقة تقرير المخزون ---
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
                                                       Quantity = subInv == null ? 0 : subInv.Quantity
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

        // --- منطقة تقرير دفتر الأستاذ ---
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
                    // حساب الرصيد الافتتاحي
                    decimal openingDebit = db.JournalVoucherItems.Where(i => i.AccountId == accountId && i.JournalVoucher.VoucherDate < fromDate).Sum(i => i.Debit);
                    decimal openingCredit = db.JournalVoucherItems.Where(i => i.AccountId == accountId && i.JournalVoucher.VoucherDate < fromDate).Sum(i => i.Credit);
                    decimal openingBalance = openingDebit - openingCredit;
                    OpeningBalanceRun.Text = openingBalance.ToString("C");

                    // جلب حركات الفترة
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

                    // حساب وعرض الإجماليات
                    decimal totalDebit = transactions.Sum(i => i.Debit);
                    decimal totalCredit = transactions.Sum(i => i.Credit);
                    TotalDebitRun.Text = totalDebit.ToString("C");
                    TotalCreditRun.Text = totalCredit.ToString("C");
                    ClosingBalanceRun.Text = currentBalance.ToString("C");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- منطقة تقرير ميزان المراجعة ---
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
                    TbTotalDebitRun.Text = grandTotalDebit.ToString("C");
                    TbTotalCreditRun.Text = grandTotalCredit.ToString("C");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // --- بداية التحديث: منطقة تقرير قائمة الدخل ---
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
                    // جلب الإيرادات
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

                    // جلب المصروفات
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

                    // حساب وعرض صافي الربح / الخسارة
                    decimal netProfitLoss = totalRevenue - totalExpense;
                    NetProfitLossRun.Text = netProfitLoss.ToString("C");
                    NetProfitLossRun.Foreground = netProfitLoss >= 0 ? Brushes.Green : Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // --- نهاية التحديث ---

        // --- بداية التحديث: منطقة تقرير الميزانية العمومية ---
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

                    // بناء شجرة الأصول
                    var assets = BuildAccountTree(AccountType.Asset, allAccounts, allTransactions);
                    AssetsTreeView.ItemsSource = assets;
                    decimal totalAssets = assets.Sum(a => a.Balance);
                    TotalAssetsRun.Text = totalAssets.ToString("C");

                    // بناء شجرة الخصوم
                    var liabilities = BuildAccountTree(AccountType.Liability, allAccounts, allTransactions);
                    LiabilitiesTreeView.ItemsSource = liabilities;
                    decimal totalLiabilities = liabilities.Sum(l => l.Balance);

                    // بناء شجرة حقوق الملكية (مع إضافة صافي الربح/الخسارة)
                    var equity = BuildAccountTree(AccountType.Equity, allAccounts, allTransactions);
                    decimal netProfitLoss = CalculateNetProfitLoss(db, toDate); // حساب صافي الربح
                    equity.Add(new BalanceSheetAccountViewModel { AccountName = "صافي الربح (الخسارة)", Balance = netProfitLoss });
                    EquityTreeView.ItemsSource = equity;
                    decimal totalEquity = equity.Sum(eq => eq.Balance);

                    TotalLiabilitiesAndEquityRun.Text = (totalLiabilities + totalEquity).ToString("C");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // دالة متكررة لبناء شجرة الحسابات
        private ObservableCollection<BalanceSheetAccountViewModel> BuildAccountTree(AccountType type, List<Account> allAccounts, List<JournalVoucherItem> allTransactions, int? parentId = null)
        {
            var nodes = new ObservableCollection<BalanceSheetAccountViewModel>();
            var childAccounts = allAccounts.Where(a => a.ParentAccountId == parentId && a.AccountType == type).ToList();

            foreach (var account in childAccounts)
            {
                decimal balance = CalculateAccountBalance(account.Id, allTransactions);
                var node = new BalanceSheetAccountViewModel
                {
                    AccountName = account.AccountName,
                    Balance = balance,
                    SubAccounts = BuildAccountTree(type, allAccounts, allTransactions, account.Id)
                };

                // إضافة رصيد الحسابات الفرعية إلى رصيد الحساب الأب
                node.Balance += node.SubAccounts.Sum(sa => sa.Balance);

                // عرض الحسابات التي لها رصيد أو التي لها حسابات فرعية
                if (node.Balance != 0 || node.SubAccounts.Any())
                {
                    nodes.Add(node);
                }
            }
            return nodes;
        }

        // دالة لحساب رصيد حساب معين
        private decimal CalculateAccountBalance(int accountId, List<JournalVoucherItem> transactions)
        {
            var accountTransactions = transactions.Where(t => t.AccountId == accountId);
            decimal totalDebit = accountTransactions.Sum(t => t.Debit);
            decimal totalCredit = accountTransactions.Sum(t => t.Credit);
            return totalDebit - totalCredit;
        }

        // دالة لحساب صافي الربح/الخسارة حتى تاريخ معين
        private decimal CalculateNetProfitLoss(DatabaseContext db, DateTime toDate)
        {
            var revenues = db.JournalVoucherItems.Include(i => i.Account)
                             .Where(i => i.Account.AccountType == AccountType.Revenue && i.JournalVoucher.VoucherDate <= toDate)
                             .Sum(i => i.Credit - i.Debit);

            var expenses = db.JournalVoucherItems.Include(i => i.Account)
                             .Where(i => i.Account.AccountType == AccountType.Expense && i.JournalVoucher.VoucherDate <= toDate)
                             .Sum(i => i.Debit - i.Credit);

            return revenues - expenses;
        }
        // *** بداية التحديث: منطقة تقارير الإنتاج ***
        private void LoadWorkOrderStatusFilter()
        {
            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(WorkOrderStatus)).Cast<object>());
            WoStatusFilterComboBox.ItemsSource = statuses;
            WoStatusFilterComboBox.SelectedIndex = 0;
        }

        private void GenerateWoReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (WoFromDatePicker.SelectedDate == null || WoToDatePicker.SelectedDate == null)
            {
                MessageBox.Show("يرجى تحديد الفترة الزمنية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime fromDate = WoFromDatePicker.SelectedDate.Value.Date;
            DateTime toDate = WoToDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.WorkOrders.Include(wo => wo.FinishedGood).AsQueryable();

                    // تطبيق فلتر الحالة
                    if (WoStatusFilterComboBox.SelectedItem != null && WoStatusFilterComboBox.SelectedItem is WorkOrderStatus status)
                    {
                        query = query.Where(wo => wo.Status == status);
                    }

                    // تطبيق فلتر التاريخ
                    query = query.Where(wo => wo.PlannedStartDate >= fromDate && wo.PlannedStartDate <= toDate);

                    WoReportDataGrid.ItemsSource = query.OrderByDescending(wo => wo.PlannedStartDate).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // --- بداية التحديث: منطقة تقرير تكلفة الإنتاج ---
        private void GeneratePcReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (PcFromDatePicker.SelectedDate == null || PcToDatePicker.SelectedDate == null)
            {
                MessageBox.Show("يرجى تحديد الفترة الزمنية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime fromDate = PcFromDatePicker.SelectedDate.Value.Date;
            DateTime toDate = PcToDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                using (var db = new DatabaseContext())
                {
                    var reportItems = new List<ProductionCostReportViewModel>();

                    // 1. جلب أوامر العمل المكتملة في الفترة المحددة
                    var completedWorkOrders = db.WorkOrders
                        .Include(wo => wo.FinishedGood)
                        .Where(wo => wo.Status == WorkOrderStatus.Completed &&
                                     wo.ActualEndDate >= fromDate && wo.ActualEndDate <= toDate)
                        .ToList();

                    foreach (var wo in completedWorkOrders)
                    {
                        // 2. جلب المواد المستهلكة لكل أمر عمل
                        var consumedMaterials = db.WorkOrderMaterials
                            .Include(m => m.RawMaterial)
                            .Where(m => m.WorkOrderId == wo.Id)
                            .ToList();

                        // 3. حساب التكلفة الإجمالية للمواد
                        decimal totalCost = consumedMaterials.Sum(m => m.QuantityConsumed * m.RawMaterial.PurchasePrice);

                        reportItems.Add(new ProductionCostReportViewModel
                        {
                            WorkOrderNumber = wo.WorkOrderNumber,
                            ProductName = wo.FinishedGood.Name,
                            ProducedQuantity = wo.QuantityProduced,
                            CompletionDate = wo.ActualEndDate.Value,
                            TotalMaterialCost = totalCost
                        });
                    }

                    PcReportDataGrid.ItemsSource = reportItems;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // --- بداية التحديث: منطقة تقرير تحليل استهلاك المواد ---
        private void LoadCompletedWorkOrders()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // جلب أوامر العمل المكتملة فقط لعرضها في القائمة
                    var completedOrders = db.WorkOrders
                        .Where(wo => wo.Status == WorkOrderStatus.Completed)
                        .OrderByDescending(wo => wo.WorkOrderNumber)
                        .ToList();
                    McWorkOrderComboBox.ItemsSource = completedOrders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل أوامر العمل: {ex.Message}");
            }
        }

        private void McWorkOrderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (McWorkOrderComboBox.SelectedItem == null)
            {
                McReportDataGrid.ItemsSource = null;
                return;
            }

            int workOrderId = (int)McWorkOrderComboBox.SelectedValue;

            try
            {
                using (var db = new DatabaseContext())
                {
                    var reportItems = new List<MaterialConsumptionReportViewModel>();
                    var workOrder = db.WorkOrders.Find(workOrderId);
                    if (workOrder == null) return;

                    // جلب قائمة المكونات لحساب الكميات المخططة
                    var bom = db.BillOfMaterials.Include(b => b.BillOfMaterialsItems).ThenInclude(i => i.RawMaterial)
                                .FirstOrDefault(b => b.FinishedGoodId == workOrder.FinishedGoodId);

                    if (bom == null) { McReportDataGrid.ItemsSource = null; return; }

                    // جلب المواد المستهلكة فعلياً لأمر العمل هذا
                    var consumedMaterials = db.WorkOrderMaterials
                        .Where(m => m.WorkOrderId == workOrderId)
                        .ToList();

                    // مقارنة المخطط بالفعلي لكل مادة في قائمة المكونات
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
            catch (Exception ex)
            {
                MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // --- بداية التحديث: منطقة تقرير كفاءة الإنتاج ---
        private void GenerateEffReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (EffFromDatePicker.SelectedDate == null || EffToDatePicker.SelectedDate == null)
            {
                MessageBox.Show("يرجى تحديد الفترة الزمنية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime fromDate = EffFromDatePicker.SelectedDate.Value.Date;
            DateTime toDate = EffToDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                using (var db = new DatabaseContext())
                {
                    var reportItems = new List<ProductionEfficiencyReportViewModel>();

                    // جلب أوامر العمل المكتملة في الفترة والتي لها تاريخ بدء وانتهاء فعلي
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
            catch (Exception ex)
            {
                MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // --- بداية التحديث: منطقة تقرير الهدر في الإنتاج ---
        private void GenerateScrapReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScrapFromDatePicker.SelectedDate == null || ScrapToDatePicker.SelectedDate == null)
            {
                MessageBox.Show("يرجى تحديد الفترة الزمنية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            catch (Exception ex)
            {
                MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // --- نهاية التحديث ---
    }
}

