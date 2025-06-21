// GoodMorningFactory/UI/ViewModels/ReportProductionViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using System;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ReportProductionViewModel : BaseViewModel
    {
        private readonly IWorkOrderService _workOrderService;
        private readonly int _workOrderId;

        #region الخصائص (Properties)
        private string _workOrderNumber;
        public string WorkOrderNumber { get => _workOrderNumber; set { _workOrderNumber = value; OnPropertyChanged(); } }

        private string _productName;
        public string ProductName { get => _productName; set { _productName = value; OnPropertyChanged(); } }

        private int _orderedQuantity;
        public int OrderedQuantity { get => _orderedQuantity; set { _orderedQuantity = value; OnPropertyChanged(); } }

        private int _previouslyProduced;
        public int PreviouslyProduced { get => _previouslyProduced; set { _previouslyProduced = value; OnPropertyChanged(); } }

        private int _remainingQuantity;
        public int RemainingQuantity { get => _remainingQuantity; set { _remainingQuantity = value; OnPropertyChanged(); } }

        private int _producedQuantity;
        public int ProducedQuantity { get => _producedQuantity; set { _producedQuantity = value; OnPropertyChanged(); } }

        private int _scrappedQuantity;
        public int ScrappedQuantity { get => _scrappedQuantity; set { _scrappedQuantity = value; OnPropertyChanged(); } }

        private string _scrapReason;
        public string ScrapReason { get => _scrapReason; set { _scrapReason = value; OnPropertyChanged(); } }
        #endregion

        #region الأوامر (Commands)
        public ICommand ConfirmProductionCommand { get; }
        #endregion

        public ReportProductionViewModel(int workOrderId)
        {
            _workOrderService = new WorkOrderService();
            _workOrderId = workOrderId;

            ConfirmProductionCommand = new RelayCommand(ConfirmProduction, CanConfirmProduction);

            LoadDataAsync();
        }

        /// <summary>
        /// تحميل البيانات الأولية اللازمة لعرضها في نافذة تسجيل الإنتاج.
        /// </summary>
        private async void LoadDataAsync()
        {
            try
            {
                var data = await _workOrderService.GetDataForProductionReportAsync(_workOrderId);
                if (data != null)
                {
                    WorkOrderNumber = data.WorkOrderNumber;
                    ProductName = data.ProductName;
                    OrderedQuantity = data.OrderedQuantity;
                    PreviouslyProduced = data.PreviouslyProduced;
                    RemainingQuantity = data.RemainingQuantity;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        /// <summary>
        /// شرط يحدد ما إذا كان يمكن تأكيد عملية تسجيل الإنتاج.
        /// يجب أن يكون إجمالي الكمية المسجلة (المنتجة + التالفة) أكبر من صفر.
        /// </summary>
        private bool CanConfirmProduction(object parameter)
        {
            return (ProducedQuantity + ScrappedQuantity) > 0;
        }

        /// <summary>
        /// يقوم بتنفيذ عملية تسجيل الإنتاج النهائية عبر استدعاء الخدمة المختصة.
        /// </summary>
        private async void ConfirmProduction(object parameter)
        {
            try
            {
                await _workOrderService.ReportProductionAsync(_workOrderId, ProducedQuantity, ScrappedQuantity, ScrapReason);
                MessageBox.Show("تم تسجيل الإنتاج بنجاح.", "نجاح");

                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية تسجيل الإنتاج: {ex.Message}", "خطأ");
            }
        }
    }
}