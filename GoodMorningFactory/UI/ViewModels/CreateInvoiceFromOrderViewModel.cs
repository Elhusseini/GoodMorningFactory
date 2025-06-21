using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel لنافذة إنشاء فاتورة جديدة من أمر بيع.
    /// </summary>
    public class CreateInvoiceFromOrderViewModel : BaseViewModel
    {
        private readonly ISalesOrderService _salesOrderService;
        private readonly int _salesOrderId;

        #region Properties
        private string _salesOrderNumber;
        public string SalesOrderNumber { get => _salesOrderNumber; set { _salesOrderNumber = value; OnPropertyChanged(); } }

        private DateTime _invoiceDate = DateTime.Today;
        public DateTime InvoiceDate { get => _invoiceDate; set { _invoiceDate = value; OnPropertyChanged(); } }

        private DateTime? _dueDate;
        public DateTime? DueDate { get => _dueDate; set { _dueDate = value; OnPropertyChanged(); } }

        public ObservableCollection<InvoiceItemViewModel> ItemsToInvoice { get; set; }
        #endregion

        #region Commands
        public ICommand CreateInvoiceCommand { get; }
        #endregion

        public CreateInvoiceFromOrderViewModel()
        {
            // مُنشئ فارغ لوقت التصميم
            SalesOrderNumber = "SO-DESIGN-TIME";
            ItemsToInvoice = new ObservableCollection<InvoiceItemViewModel>();
        }

        public CreateInvoiceFromOrderViewModel(int salesOrderId)
        {
            _salesOrderService = new SalesOrderService();
            _salesOrderId = salesOrderId;
            ItemsToInvoice = new ObservableCollection<InvoiceItemViewModel>();
            CreateInvoiceCommand = new RelayCommand(CreateInvoiceAsync, CanCreateInvoice);
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                var invoiceData = await _salesOrderService.GetDataForInvoicingAsync(_salesOrderId);
                if (invoiceData != null)
                {
                    SalesOrderNumber = invoiceData.SalesOrderNumber;
                    DueDate = DateTime.Today.AddDays(invoiceData.PaymentTerms);
                    foreach (var item in invoiceData.InvoiceableItems)
                    {
                        ItemsToInvoice.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل بيانات الفوترة: {ex.Message}", "خطأ");
            }
        }

        private async void CreateInvoiceAsync(object parameter)
        {
            try
            {
                await _salesOrderService.CreateInvoiceFromOrderAsync(_salesOrderId, InvoiceDate, DueDate, ItemsToInvoice);
                MessageBox.Show("تم إنشاء الفاتورة بنجاح.", "نجاح");
                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية إنشاء الفاتورة: {ex.Message}", "خطأ");
            }
        }

        private bool CanCreateInvoice(object parameter)
        {
            // التأكد من وجود أصناف لفوترتها
            return ItemsToInvoice != null && ItemsToInvoice.Any(i => i.QuantityToInvoice > 0);
        }
    }
}
