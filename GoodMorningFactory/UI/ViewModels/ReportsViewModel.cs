// UI/ViewModels/ReportsViewModel.cs
// *** الكود الكامل والمصحح لـ ViewModel الخاص بشاشة التقارير ***

using GoodMorningFactory.Core.Documents;
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using Microsoft.Win32;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        #region الخدمات والبيانات الأساسية
        private readonly IReportsService _reportsService;
        private List<Sale> _currentSalesReportData = new List<Sale>();
        private List<Purchase> _currentPurchasesReportData = new List<Purchase>();
        private List<InventoryViewModel> _currentInventoryReportData = new List<InventoryViewModel>();
        #endregion

        #region الخصائص العامة (Properties)
        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }
        #endregion

        #region خصائص وأوامر تقرير المبيعات
        public DateTime SalesFromDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DateTime SalesToDate { get; set; } = DateTime.Now;
        public ObservableCollection<Sale> SalesReportData { get; set; } = new ObservableCollection<Sale>();
        private string _totalSalesText;
        public string TotalSalesText { get => _totalSalesText; set { _totalSalesText = value; OnPropertyChanged(); } }

        public ICommand GenerateSalesReportCommand { get; }
        public ICommand ExportSalesToPdfCommand { get; }
        #endregion

        #region خصائص وأوامر تقرير المشتريات
        public DateTime PurchasesFromDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DateTime PurchasesToDate { get; set; } = DateTime.Now;
        public ObservableCollection<Purchase> PurchasesReportData { get; set; } = new ObservableCollection<Purchase>();
        private string _totalPurchasesText;
        public string TotalPurchasesText { get => _totalPurchasesText; set { _totalPurchasesText = value; OnPropertyChanged(); } }

        public ICommand GeneratePurchasesReportCommand { get; }
        public ICommand ExportPurchasesToPdfCommand { get; }
        #endregion

        #region خصائص وأوامر تقرير المخزون
        public ObservableCollection<InventoryViewModel> InventoryReportData { get; set; } = new ObservableCollection<InventoryViewModel>();
        public ICommand ExportInventoryToPdfCommand { get; }
        public ICommand RefreshInventoryReportCommand { get; }
        #endregion

        #region خصائص وأوامر دفتر الأستاذ العام
        public DateTime GlFromDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DateTime GlToDate { get; set; } = DateTime.Now;
        public ObservableCollection<Account> GlAccounts { get; set; } = new ObservableCollection<Account>();
        public Account SelectedGlAccount { get; set; }
        public ObservableCollection<GeneralLedgerItemViewModel> GlReportData { get; set; } = new ObservableCollection<GeneralLedgerItemViewModel>();
        private string _glOpeningBalanceText, _glTotalDebitText, _glTotalCreditText, _glClosingBalanceText;
        public string GlOpeningBalanceText { get => _glOpeningBalanceText; set { _glOpeningBalanceText = value; OnPropertyChanged(); } }
        public string GlTotalDebitText { get => _glTotalDebitText; set { _glTotalDebitText = value; OnPropertyChanged(); } }
        public string GlTotalCreditText { get => _glTotalCreditText; set { _glTotalCreditText = value; OnPropertyChanged(); } }
        public string GlClosingBalanceText { get => _glClosingBalanceText; set { _glClosingBalanceText = value; OnPropertyChanged(); } }

        public ICommand GenerateGlReportCommand { get; }
        #endregion

        #region خصائص وأوامر ميزان المراجعة
        public DateTime TbDate { get; set; } = DateTime.Now;
        public ObservableCollection<TrialBalanceItemViewModel> TbReportData { get; set; } = new ObservableCollection<TrialBalanceItemViewModel>();
        private string _tbTotalDebitText, _tbTotalCreditText;
        public string TbTotalDebitText { get => _tbTotalDebitText; set { _tbTotalDebitText = value; OnPropertyChanged(); } }
        public string TbTotalCreditText { get => _tbTotalCreditText; set { _tbTotalCreditText = value; OnPropertyChanged(); } }

        public ICommand GenerateTbReportCommand { get; }
        #endregion

        #region خصائص وأوامر قائمة الدخل
        public DateTime IsFromDate { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);
        public DateTime IsToDate { get; set; } = DateTime.Now;
        public ObservableCollection<IncomeStatementItemViewModel> RevenueItems { get; set; } = new ObservableCollection<IncomeStatementItemViewModel>();
        public ObservableCollection<IncomeStatementItemViewModel> ExpenseItems { get; set; } = new ObservableCollection<IncomeStatementItemViewModel>();
        private string _netProfitLossText;
        public string NetProfitLossText { get => _netProfitLossText; set { _netProfitLossText = value; OnPropertyChanged(); } }

        public ICommand GenerateIsReportCommand { get; }
        #endregion

        #region خصائص وأوامر الميزانية العمومية
        public DateTime BsDate { get; set; } = DateTime.Now;
        public ObservableCollection<BalanceSheetAccountViewModel> Assets { get; set; } = new ObservableCollection<BalanceSheetAccountViewModel>();
        public ObservableCollection<BalanceSheetAccountViewModel> Liabilities { get; set; } = new ObservableCollection<BalanceSheetAccountViewModel>();
        public ObservableCollection<BalanceSheetAccountViewModel> Equity { get; set; } = new ObservableCollection<BalanceSheetAccountViewModel>();
        private string _totalAssetsText, _totalLiabilitiesAndEquityText;
        public string TotalAssetsText { get => _totalAssetsText; set { _totalAssetsText = value; OnPropertyChanged(); } }
        public string TotalLiabilitiesAndEquityText { get => _totalLiabilitiesAndEquityText; set { _totalLiabilitiesAndEquityText = value; OnPropertyChanged(); } }

        public ICommand GenerateBsReportCommand { get; }
        #endregion

        #region خصائص وأوامر التدفقات النقدية
        public DateTime CfFromDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DateTime CfToDate { get; set; } = DateTime.Now;
        public ObservableCollection<CashFlowItemViewModel> CashFlowItems { get; set; } = new ObservableCollection<CashFlowItemViewModel>();

        public ICommand GenerateCfReportCommand { get; }
        #endregion

        #region خصائص وأوامر تقارير الإنتاج
        // أوامر العمل
        public DateTime WoFromDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DateTime WoToDate { get; set; } = DateTime.Now;
        public List<object> WoStatusFilter { get; set; }
        public object SelectedWoStatus { get; set; }
        public ObservableCollection<WorkOrder> WoReportData { get; set; } = new ObservableCollection<WorkOrder>();
        public ICommand GenerateWoReportCommand { get; }

        // تكاليف الإنتاج
        public DateTime PcFromDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DateTime PcToDate { get; set; } = DateTime.Now;
        public ObservableCollection<ProductionCostReportViewModel> PcReportData { get; set; } = new ObservableCollection<ProductionCostReportViewModel>();
        public ICommand GeneratePcReportCommand { get; }

        // استهلاك المواد
        public ObservableCollection<WorkOrder> CompletedWorkOrders { get; set; } = new ObservableCollection<WorkOrder>();
        private WorkOrder _selectedMcWorkOrder;
        public WorkOrder SelectedMcWorkOrder
        {
            get => _selectedMcWorkOrder;
            set
            {
                if (_selectedMcWorkOrder != value)
                {
                    _selectedMcWorkOrder = value;
                    OnPropertyChanged();
                    if (_selectedMcWorkOrder != null)
                    {
                        GenerateMcReportCommand.Execute(null);
                    }
                }
            }
        }
        public ObservableCollection<MaterialConsumptionReportViewModel> McReportData { get; set; } = new ObservableCollection<MaterialConsumptionReportViewModel>();
        public ICommand GenerateMcReportCommand { get; }

        // كفاءة الإنتاج
        public DateTime EffFromDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DateTime EffToDate { get; set; } = DateTime.Now;
        public ObservableCollection<ProductionEfficiencyReportViewModel> EffReportData { get; set; } = new ObservableCollection<ProductionEfficiencyReportViewModel>();
        public ICommand GenerateEffReportCommand { get; }

        // الهدر
        public DateTime ScrapFromDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DateTime ScrapToDate { get; set; } = DateTime.Now;
        public ObservableCollection<ScrapReportViewModel> ScrapReportData { get; set; } = new ObservableCollection<ScrapReportViewModel>();
        public ICommand GenerateScrapReportCommand { get; }
        #endregion

        #region خصائص وأوامر تقرير مراكز التكلفة
        public DateTime CcFromDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DateTime CcToDate { get; set; } = DateTime.Now;
        public ObservableCollection<object> CostCenterFilter { get; set; } = new ObservableCollection<object>();
        public object SelectedCostCenter { get; set; }
        public ObservableCollection<CostCenterReportViewModel> CcReportData { get; set; } = new ObservableCollection<CostCenterReportViewModel>();
        private string _ccTotalRevenue, _ccTotalExpense, _ccTotalNet;
        public string CcTotalRevenue { get => _ccTotalRevenue; set { _ccTotalRevenue = value; OnPropertyChanged(); } }
        public string CcTotalExpense { get => _ccTotalExpense; set { _ccTotalExpense = value; OnPropertyChanged(); } }
        public string CcTotalNet { get => _ccTotalNet; set { _ccTotalNet = value; OnPropertyChanged(); } }

        public ICommand GenerateCcReportCommand { get; }
        #endregion

        #region خصائص وأوامر تقرير الموازنة
        public ObservableCollection<Budget> BudgetFilter { get; set; } = new ObservableCollection<Budget>();
        public Budget SelectedBudget { get; set; }
        public List<object> BudgetMonthFilter { get; }
        public object SelectedBudgetMonth { get; set; }
        public ObservableCollection<BudgetVsActualViewModel> BudgetVsActualData { get; set; } = new ObservableCollection<BudgetVsActualViewModel>();
        private string _budgetTotal, _actualTotal, _varianceTotal;
        public string BudgetTotal { get => _budgetTotal; set { _budgetTotal = value; OnPropertyChanged(); } }
        public string ActualTotal { get => _actualTotal; set { _actualTotal = value; OnPropertyChanged(); } }
        public string VarianceTotal { get => _varianceTotal; set { _varianceTotal = value; OnPropertyChanged(); } }

        public ICommand GenerateBudgetVsActualReportCommand { get; }
        #endregion

        #region المُنشئ (Constructor)
        public ReportsViewModel()
        {
            _reportsService = new ReportsService();
            IsLoading = true;

            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(WorkOrderStatus)).Cast<object>());
            WoStatusFilter = statuses;
            SelectedWoStatus = WoStatusFilter.First();

            BudgetMonthFilter = Enumerable.Range(1, 12).Select(m => new { Month = m, Name = new DateTime(2000, m, 1).ToString("MMMM", new CultureInfo("ar-EG")) }).ToList<object>();
            SelectedBudgetMonth = BudgetMonthFilter.FirstOrDefault(m => (int)m.GetType().GetProperty("Month").GetValue(m) == DateTime.Now.Month);

            // **تصحيح**: استخدام AsyncRelayCommand للدوال غير المتزامنة و RelayCommand للدوال المتزامنة
            GenerateSalesReportCommand = new AsyncRelayCommand(GenerateSalesReportAsync);
            // *** تصحيح الخطأ CS1593 هنا ***
            ExportSalesToPdfCommand = new RelayCommand(_ => ExportSalesToPdf(), _ => SalesReportData.Any());

            GeneratePurchasesReportCommand = new AsyncRelayCommand(GeneratePurchasesReportAsync);
            // *** تصحيح الخطأ CS1593 هنا ***
            ExportPurchasesToPdfCommand = new RelayCommand(_ => ExportPurchasesToPdf(), _ => PurchasesReportData.Any());

            RefreshInventoryReportCommand = new AsyncRelayCommand(LoadInventoryReportAsync);
            // *** تصحيح الخطأ CS1593 هنا ***
            ExportInventoryToPdfCommand = new RelayCommand(_ => ExportInventoryToPdf(), _ => InventoryReportData.Any());

            GenerateGlReportCommand = new AsyncRelayCommand(GenerateGlReportAsync);
            GenerateTbReportCommand = new AsyncRelayCommand(GenerateTbReportAsync);
            GenerateIsReportCommand = new AsyncRelayCommand(GenerateIsReportAsync);
            GenerateBsReportCommand = new AsyncRelayCommand(GenerateBsReportAsync);
            GenerateCfReportCommand = new AsyncRelayCommand(GenerateCfReportAsync);
            GenerateWoReportCommand = new AsyncRelayCommand(GenerateWoReportAsync);
            GeneratePcReportCommand = new AsyncRelayCommand(GeneratePcReportAsync);
            GenerateMcReportCommand = new AsyncRelayCommand(GenerateMcReportAsync);
            GenerateEffReportCommand = new AsyncRelayCommand(GenerateEffReportAsync);
            GenerateScrapReportCommand = new AsyncRelayCommand(GenerateScrapReportAsync);
            GenerateCcReportCommand = new AsyncRelayCommand(GenerateCcReportAsync);
            GenerateBudgetVsActualReportCommand = new AsyncRelayCommand(GenerateBudgetVsActualReportAsync);

            _ = InitializeAsync();
        }
        #endregion

        #region دوال التنفيذ والتحميل

        private async Task InitializeAsync()
        {
            await LoadInventoryReportAsync();
            await LoadGlAccountsAsync();
            await LoadCompletedWorkOrdersAsync();
            await LoadCostCentersAsync();
            await LoadBudgetsAsync();
            IsLoading = false;
        }

        private async Task GenerateSalesReportAsync()
        {
            IsLoading = true;
            try
            {
                var data = await _reportsService.GetSalesReportDataAsync(SalesFromDate, SalesToDate.Date.AddDays(1).AddTicks(-1));
                _currentSalesReportData = data;
                SalesReportData.Clear();
                // **تصحيح**: استخدام حلقة foreach بدلاً من .ForEach
                foreach (var item in data)
                {
                    SalesReportData.Add(item);
                }
                TotalSalesText = $"{SalesReportData.Sum(s => s.TotalAmount):N2} {AppSettings.DefaultCurrencySymbol}";
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
            finally { IsLoading = false; }
        }

        private void ExportSalesToPdf()
        {
            SaveFileDialog sfd = new SaveFileDialog { Filter = "PDF Document (*.pdf)|*.pdf", FileName = $"SalesReport_{DateTime.Now:yyyy-MM-dd}.pdf" };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    // **تصحيح**: استخدام CompanyName بدلاً من Name
                    var companyInfo = new CompanyInfo { CompanyName = "مصنع صباح الخير" };
                    var period = $"الفترة من: {SalesFromDate:d} إلى: {SalesToDate:d}";
                    var document = new SalesReportDocument(_currentSalesReportData, companyInfo, period);
                    document.GeneratePdf(sfd.FileName);
                    MessageBox.Show("تم تصدير تقرير المبيعات بنجاح.", "نجاح");
                }
                catch (Exception ex) { MessageBox.Show($"فشل التصدير: {ex.Message}", "خطأ"); }
            }
        }

        private async Task GeneratePurchasesReportAsync()
        {
            IsLoading = true;
            try
            {
                var data = await _reportsService.GetPurchasesReportDataAsync(PurchasesFromDate, PurchasesToDate.Date.AddDays(1).AddTicks(-1));
                _currentPurchasesReportData = data;
                PurchasesReportData.Clear();
                foreach (var item in data)
                {
                    PurchasesReportData.Add(item);
                }
                TotalPurchasesText = $"{PurchasesReportData.Sum(p => p.TotalAmount):N2} {AppSettings.DefaultCurrencySymbol}";
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private void ExportPurchasesToPdf()
        {
            SaveFileDialog sfd = new SaveFileDialog { Filter = "PDF Document (*.pdf)|*.pdf", FileName = $"PurchaseReport_{DateTime.Now:yyyy-MM-dd}.pdf" };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    // **تصحيح**: استخدام CompanyName بدلاً من Name
                    var companyInfo = new CompanyInfo { CompanyName = "مصنع صباح الخير" };
                    var period = $"الفترة من: {PurchasesFromDate:d} إلى: {PurchasesToDate:d}";
                    var document = new PurchaseReportDocument(_currentPurchasesReportData, companyInfo, period);
                    document.GeneratePdf(sfd.FileName);
                    MessageBox.Show("تم تصدير تقرير المشتريات بنجاح.", "نجاح");
                }
                catch (Exception ex) { MessageBox.Show($"فشل التصدير: {ex.Message}", "خطأ"); }
            }
        }

        private async Task LoadInventoryReportAsync()
        {
            IsLoading = true;
            try
            {
                var data = await _reportsService.GetInventoryReportDataAsync();
                _currentInventoryReportData = data;
                InventoryReportData.Clear();
                foreach (var item in data)
                {
                    InventoryReportData.Add(item);
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل المخزون: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private void ExportInventoryToPdf()
        {
            SaveFileDialog sfd = new SaveFileDialog { Filter = "PDF Document (*.pdf)|*.pdf", FileName = $"InventoryReport_{DateTime.Now:yyyy-MM-dd}.pdf" };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    // **تصحيح**: استخدام CompanyName بدلاً من Name
                    var companyInfo = new CompanyInfo { CompanyName = "مصنع صباح الخير" };
                    var document = new InventoryReportDocument(_currentInventoryReportData, companyInfo);
                    document.GeneratePdf(sfd.FileName);
                    MessageBox.Show("تم تصدير تقرير المخزون بنجاح.", "نجاح");
                }
                catch (Exception ex) { MessageBox.Show($"فشل التصدير: {ex.Message}", "خطأ"); }
            }
        }

        private async Task GenerateGlReportAsync()
        {
            if (SelectedGlAccount == null) { MessageBox.Show("يرجى اختيار حساب أولاً.", "تنبيه"); return; }
            IsLoading = true;
            try
            {
                var reportItems = await _reportsService.GetGeneralLedgerReportAsync(SelectedGlAccount.Id, GlFromDate, GlToDate.Date.AddDays(1).AddTicks(-1));
                GlReportData.Clear();
                foreach (var item in reportItems)
                {
                    GlReportData.Add(item);
                }

                var openingItem = GlReportData.FirstOrDefault();
                GlOpeningBalanceText = $"{openingItem?.Balance:N2} {AppSettings.DefaultCurrencySymbol}";

                var transactions = GlReportData.Skip(1).ToList();
                GlTotalDebitText = $"{transactions.Sum(i => i.Debit):N2} {AppSettings.DefaultCurrencySymbol}";
                GlTotalCreditText = $"{transactions.Sum(i => i.Credit):N2} {AppSettings.DefaultCurrencySymbol}";
                GlClosingBalanceText = $"{GlReportData.LastOrDefault()?.Balance:N2} {AppSettings.DefaultCurrencySymbol}";
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private async Task GenerateTbReportAsync()
        {
            IsLoading = true;
            try
            {
                var reportItems = await _reportsService.GetTrialBalanceReportAsync(TbDate.Date.AddDays(1).AddTicks(-1));
                TbReportData.Clear();
                foreach (var item in reportItems)
                {
                    TbReportData.Add(item);
                }

                TbTotalDebitText = $"{TbReportData.Sum(i => i.DebitBalance):N2} {AppSettings.DefaultCurrencySymbol}";
                TbTotalCreditText = $"{TbReportData.Sum(i => i.CreditBalance):N2} {AppSettings.DefaultCurrencySymbol}";
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private async Task GenerateIsReportAsync()
        {
            IsLoading = true;
            try
            {
                var report = await _reportsService.GetIncomeStatementReportAsync(IsFromDate, IsToDate.Date.AddDays(1).AddTicks(-1));
                RevenueItems.Clear();
                // **تصحيح**: استخدام foreach بدلاً من .ForEach
                foreach (var item in report.Revenues)
                {
                    RevenueItems.Add(item);
                }

                ExpenseItems.Clear();
                // **تصحيح**: استخدام foreach بدلاً من .ForEach
                foreach (var item in report.Expenses)
                {
                    ExpenseItems.Add(item);
                }

                // **تصحيح**: استخدام NetIncome بدلاً من NetProfitLoss
                NetProfitLossText = $"{report.NetIncome:N2} {AppSettings.DefaultCurrencySymbol}";
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private async Task GenerateBsReportAsync()
        {
            IsLoading = true;
            try
            {
                var toDate = BsDate.Date.AddDays(1).AddTicks(-1);
                var assetsData = await _reportsService.GetBalanceSheetAssetsAsync(toDate);
                var (liabilitiesData, equityData) = await _reportsService.GetBalanceSheetLiabilitiesAndEquityAsync(toDate);

                Assets.Clear();
                foreach (var item in assetsData) { Assets.Add(item); }
                Liabilities.Clear();
                foreach (var item in liabilitiesData) { Liabilities.Add(item); }
                Equity.Clear();
                foreach (var item in equityData) { Equity.Add(item); }

                TotalAssetsText = $"{Assets.Sum(a => a.Balance):N2} {AppSettings.DefaultCurrencySymbol}";
                TotalLiabilitiesAndEquityText = $"{(Liabilities.Sum(l => l.Balance) + Equity.Sum(e => e.Balance)):N2} {AppSettings.DefaultCurrencySymbol}";
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private async Task GenerateCfReportAsync()
        {
            IsLoading = true;
            try
            {
                var items = await _reportsService.GetCashFlowReportAsync(CfFromDate, CfToDate.Date.AddDays(1).AddTicks(-1));
                CashFlowItems.Clear();
                foreach (var item in items) { CashFlowItems.Add(item); }
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private async Task GenerateWoReportAsync()
        {
            IsLoading = true;
            try
            {
                WorkOrderStatus? status = null;
                if (SelectedWoStatus is WorkOrderStatus selectedStatus)
                {
                    status = selectedStatus;
                }

                var items = await _reportsService.GetWorkOrdersReportAsync(WoFromDate, WoToDate.Date.AddDays(1).AddTicks(-1), status);
                WoReportData.Clear();
                foreach (var item in items) { WoReportData.Add(item); }
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private async Task GeneratePcReportAsync()
        {
            IsLoading = true;
            try
            {
                var items = await _reportsService.GetProductionCostReportAsync(PcFromDate, PcToDate.Date.AddDays(1).AddTicks(-1));
                PcReportData.Clear();
                foreach (var item in items) { PcReportData.Add(item); }
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private async Task GenerateMcReportAsync()
        {
            if (SelectedMcWorkOrder == null) return;
            IsLoading = true;
            try
            {
                var items = await _reportsService.GetMaterialConsumptionReportAsync(SelectedMcWorkOrder.Id);
                McReportData.Clear();
                foreach (var item in items) { McReportData.Add(item); }
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private async Task GenerateEffReportAsync()
        {
            IsLoading = true;
            try
            {
                var items = await _reportsService.GetProductionEfficiencyReportAsync(EffFromDate, EffToDate.Date.AddDays(1).AddTicks(-1));
                EffReportData.Clear();
                foreach (var item in items) { EffReportData.Add(item); }
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private async Task GenerateScrapReportAsync()
        {
            IsLoading = true;
            try
            {
                var items = await _reportsService.GetScrapReportAsync(ScrapFromDate, ScrapToDate.Date.AddDays(1).AddTicks(-1));
                ScrapReportData.Clear();
                foreach (var item in items) { ScrapReportData.Add(item); }
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private async Task GenerateCcReportAsync()
        {
            IsLoading = true;
            try
            {
                int? centerId = null;
                if (SelectedCostCenter != null && (int)SelectedCostCenter.GetType().GetProperty("Id").GetValue(SelectedCostCenter) > 0)
                {
                    centerId = (int)SelectedCostCenter.GetType().GetProperty("Id").GetValue(SelectedCostCenter);
                }

                var items = await _reportsService.GetCostCenterReportAsync(CcFromDate, CcToDate.Date.AddDays(1).AddTicks(-1), centerId);
                CcReportData.Clear();
                foreach (var item in items) { CcReportData.Add(item); }

                CcTotalRevenue = $"{items.Sum(r => r.TotalRevenue):N2} {AppSettings.DefaultCurrencySymbol}";
                CcTotalExpense = $"{items.Sum(r => r.TotalExpenses):N2} {AppSettings.DefaultCurrencySymbol}";
                CcTotalNet = $"{items.Sum(r => r.NetProfitLoss):N2} {AppSettings.DefaultCurrencySymbol}";
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private async Task GenerateBudgetVsActualReportAsync()
        {
            if (SelectedBudget == null || SelectedBudgetMonth == null) { MessageBox.Show("يرجى اختيار الموازنة والشهر.", "تنبيه"); return; }
            IsLoading = true;
            try
            {
                int month = (int)SelectedBudgetMonth.GetType().GetProperty("Month").GetValue(SelectedBudgetMonth);
                int year = SelectedBudget.Year;

                var items = await _reportsService.GetBudgetVsActualReportAsync(SelectedBudget.Id, month, year);
                BudgetVsActualData.Clear();
                foreach (var item in items) { BudgetVsActualData.Add(item); }

                BudgetTotal = $"{items.Sum(r => r.BudgetedAmount):N2} {AppSettings.DefaultCurrencySymbol}";
                ActualTotal = $"{items.Sum(r => r.ActualAmount):N2} {AppSettings.DefaultCurrencySymbol}";
                VarianceTotal = $"{items.Sum(r => r.Variance):N2} {AppSettings.DefaultCurrencySymbol}";
            }
            catch (Exception ex) { MessageBox.Show($"فشل توليد التقرير: {ex.Message}", "خطأ"); }
            finally { IsLoading = false; }
        }

        private async Task LoadGlAccountsAsync()
        {
            try
            {
                var accounts = await _reportsService.GetAccountsForFilterAsync();
                GlAccounts.Clear();
                foreach (var item in accounts) { GlAccounts.Add(item); }
                SelectedGlAccount = GlAccounts.FirstOrDefault();
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل الحسابات: {ex.Message}"); }
        }

        private async Task LoadCompletedWorkOrdersAsync()
        {
            try
            {
                var orders = await _reportsService.GetCompletedWorkOrdersForFilterAsync();
                CompletedWorkOrders.Clear();
                foreach (var item in orders) { CompletedWorkOrders.Add(item); }
                SelectedMcWorkOrder = CompletedWorkOrders.FirstOrDefault();
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل أوامر العمل: {ex.Message}"); }
        }

        private async Task LoadCostCentersAsync()
        {
            try
            {
                var costCenters = await _reportsService.GetCostCentersForFilterAsync();
                CostCenterFilter.Clear();
                CostCenterFilter.Add(new { Name = "الكل", Id = 0 });
                foreach (var item in costCenters) { CostCenterFilter.Add(new { Name = item.Name, Id = item.Id }); }
                SelectedCostCenter = CostCenterFilter.FirstOrDefault();
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل مراكز التكلفة: {ex.Message}"); }
        }

        private async Task LoadBudgetsAsync()
        {
            try
            {
                var budgets = await _reportsService.GetBudgetsForFilterAsync();
                BudgetFilter.Clear();
                foreach (var item in budgets) { BudgetFilter.Add(item); }
                SelectedBudget = BudgetFilter.FirstOrDefault();
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل الموازنات: {ex.Message}"); }
        }

        #endregion
    }
}