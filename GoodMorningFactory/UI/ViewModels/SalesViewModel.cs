// GoodMorningFactory/UI/ViewModels/SalesViewModel.cs
using GoodMorningFactory.Core.Helpers;
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class SalesViewModel : BaseViewModel
    {
        #region الخدمات (Services)
        private readonly ISalesService _salesService;
        private readonly ICustomerService _customerService;
        private readonly IPrintingService _printingService;
        #endregion

        #region الخصائص (Properties)
        private ObservableCollection<SalesItemViewModel> _sales;
        public ObservableCollection<SalesItemViewModel> Sales
        {
            get => _sales;
            set { _sales = value; OnPropertyChanged(); }
        }

        public List<object> Statuses { get; private set; }
        public List<Customer> Customers { get; private set; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ResetAndLoadSales(); }
        }

        private object _selectedStatus;
        public object SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); ResetAndLoadSales(); }
        }

        private Customer _selectedCustomer;
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); ResetAndLoadSales(); }
        }

        private DateTime? _dueDateFrom;
        public DateTime? DueDateFrom
        {
            get => _dueDateFrom;
            set { _dueDateFrom = value; OnPropertyChanged(); ResetAndLoadSales(); }
        }

        private DateTime? _dueDateTo;
        public DateTime? DueDateTo
        {
            get => _dueDateTo;
            set { _dueDateTo = value; OnPropertyChanged(); ResetAndLoadSales(); }
        }
        #endregion

        #region الأوامر (Commands)
        public ICommand NewSaleCommand { get; }
        public ICommand EditSaleCommand { get; }
        public ICommand RecordPaymentCommand { get; }
        public ICommand CreateReturnCommand { get; }
        public ICommand PrintInvoiceCommand { get; }
        public ICommand ExportToCsvCommand { get; }
        #endregion

        #region المُنشئ (Constructor)
        public SalesViewModel()
        {
            _salesService = new SalesService();
            _customerService = new CustomerService();
            _printingService = new PrintingService();

            Sales = new ObservableCollection<SalesItemViewModel>();

            NewSaleCommand = new RelayCommand(ExecuteNewSale);
            EditSaleCommand = new RelayCommand(ExecuteEditSale, CanExecuteEditSale);
            RecordPaymentCommand = new RelayCommand(ExecuteRecordPayment, CanExecuteActions);
            CreateReturnCommand = new RelayCommand(ExecuteCreateReturn, CanExecuteActions);
            PrintInvoiceCommand = new RelayCommand(ExecutePrintInvoice, CanExecuteActions);

            // ---  بداية التصحيح الرئيسي  ---
            // تم تصحيح طريقة تعريف هذا الأمر لتتوافق مع RelayCommand
            ExportToCsvCommand = new RelayCommand(ExecuteExportToCsv, _ => CanExecuteExport());
            // ---  نهاية التصحيح الرئيسي  ---

            InitializeData();
        }

        private async void InitializeData()
        {
            await LoadFilters();
            await LoadSales();
        }
        #endregion

        #region دوال تنفيذ الأوامر (Command Implementations)
        private async void ExecuteNewSale(object parameter)
        {
            var newSaleWindow = new AddEditSaleWindow();
            if (newSaleWindow.ShowDialog() == true)
            {
                await LoadSales();
            }
        }

        private async void ExecuteEditSale(object parameter)
        {
            if (parameter is SalesItemViewModel selectedSale)
            {
                var editWindow = new AddEditSaleWindow(selectedSale.Id);
                if (editWindow.ShowDialog() == true)
                {
                    await LoadSales();
                }
            }
        }

        private bool CanExecuteEditSale(object parameter)
        {
            return parameter is SalesItemViewModel selectedSale
                   && selectedSale.AmountPaid == 0
                   && selectedSale.Status != InvoiceStatus.Cancelled;
        }

        private async void ExecuteRecordPayment(object parameter)
        {
            if (parameter is SalesItemViewModel selectedSale)
            {
                var paymentWindow = new RecordPaymentWindow(selectedSale.Id);
                if (paymentWindow.ShowDialog() == true)
                {
                    await LoadSales();
                }
            }
        }

        private async void ExecuteCreateReturn(object parameter)
        {
            if (parameter is SalesItemViewModel selectedSale)
            {
                var returnWindow = new AddSalesReturnWindow(selectedSale.Id);
                if (returnWindow.ShowDialog() == true)
                {
                    await LoadSales();
                }
            }
        }

        private bool CanExecuteActions(object parameter)
        {
            return parameter is SalesItemViewModel;
        }

        private async void ExecutePrintInvoice(object parameter)
        {
            if (parameter is SalesItemViewModel selectedSale)
            {
                await _printingService.PrintSalesInvoiceAsync(selectedSale.Id);
            }
        }

        private void ExecuteExportToCsv(object parameter)
        {
            if (Sales == null || !Sales.Any())
            {
                MessageBox.Show("لا توجد بيانات لتصديرها.", "تصدير");
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "CSV (Comma delimited)|*.csv",
                Title = "حفظ تقرير الفواتير",
                FileName = $"Sales_Report_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("رقم الفاتورة,العميل,تاريخ الفاتورة,تاريخ الاستحقاق,الإجمالي,المدفوع,الرصيد,الحالة");

                    foreach (var sale in Sales)
                    {
                        // ---  بداية التصحيح الرئيسي  ---
                        // تم استخدام الخاصية الجديدة StatusDescription
                        sb.AppendLine($"\"{sale.InvoiceNumber}\",\"{sale.CustomerName}\",{sale.SaleDate:yyyy-MM-dd},{sale.DueDate:yyyy-MM-dd},{sale.TotalAmount},{sale.AmountPaid},{sale.BalanceDue},\"{sale.StatusDescription}\"");
                        // ---  نهاية التصحيح الرئيسي  ---
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show($"تم تصدير البيانات بنجاح إلى:\n{sfd.FileName}", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشلت عملية التصدير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanExecuteExport()
        {
            return PermissionsService.CanAccess("Reports.Export");
        }
        #endregion

        #region دوال مساعدة (Helper Methods)
        private async Task LoadFilters()
        {
            try
            {
                var statuses = new List<object> { "الكل" };
                statuses.AddRange(Enum.GetValues(typeof(InvoiceStatus)).Cast<object>());
                Statuses = statuses;
                _selectedStatus = Statuses.First();
                OnPropertyChanged(nameof(Statuses));
                OnPropertyChanged(nameof(SelectedStatus));

                var customers = new List<Customer> { new Customer { CustomerName = "الكل", Id = 0 } };
                var activeCustomers = await _customerService.GetActiveCustomersAsync();
                customers.AddRange(activeCustomers);
                Customers = customers;
                _selectedCustomer = Customers.First();
                OnPropertyChanged(nameof(Customers));
                OnPropertyChanged(nameof(SelectedCustomer));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل الفلاتر: {ex.Message}", "خطأ");
            }
        }

        private async Task LoadSales()
        {
            try
            {
                InvoiceStatus? statusFilter = (SelectedStatus is InvoiceStatus status) ? status : (InvoiceStatus?)null;
                int? customerIdFilter = (SelectedCustomer != null && SelectedCustomer.Id > 0) ? SelectedCustomer.Id : (int?)null;

                var salesSummaryData = await _salesService.GetSalesAsync(SearchText, statusFilter, customerIdFilter, DueDateFrom, DueDateTo);

                // ---  بداية التصحيح الرئيسي  ---
                // تم استخدام المُنشئ الجديد الذي يقبل كائن البيانات
                var salesViewModels = salesSummaryData.Select(summary => new SalesItemViewModel(summary)).ToList();
                // ---  نهاية التصحيح الرئيسي  ---

                Sales = new ObservableCollection<SalesItemViewModel>(salesViewModels);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الفواتير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetAndLoadSales()
        {
            Task.Run(async () => await LoadSales());
        }
        #endregion
    }
}