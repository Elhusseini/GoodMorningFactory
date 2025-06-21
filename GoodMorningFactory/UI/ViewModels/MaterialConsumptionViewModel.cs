// GoodMorningFactory/UI/ViewModels/MaterialConsumptionViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class MaterialConsumptionViewModel : BaseViewModel
    {
        private readonly IWorkOrderService _workOrderService;
        private readonly int _workOrderId;

        #region الخصائص (Properties)
        private string _workOrderNumber;
        public string WorkOrderNumber { get => _workOrderNumber; set { _workOrderNumber = value; OnPropertyChanged(); } }

        public ObservableCollection<RequiredMaterialViewModel> ItemsToConsume { get; set; }
        #endregion

        #region الأوامر (Commands)
        public ICommand ConfirmConsumptionCommand { get; }
        public ICommand SelectTrackingDataCommand { get; }
        #endregion

        public MaterialConsumptionViewModel(int workOrderId)
        {
            _workOrderService = new WorkOrderService();
            _workOrderId = workOrderId;
            ItemsToConsume = new ObservableCollection<RequiredMaterialViewModel>();

            ConfirmConsumptionCommand = new RelayCommand(ConfirmConsumption, CanConfirmConsumption);
            SelectTrackingDataCommand = new RelayCommand(SelectTrackingData);

            LoadDataAsync();
        }

        /// <summary>
        /// تحميل البيانات الأولية اللازمة لعرضها في نافذة صرف المواد.
        /// </summary>
        private async void LoadDataAsync()
        {
            try
            {
                var data = await _workOrderService.GetDataForMaterialConsumptionAsync(_workOrderId);
                if (data != null)
                {
                    WorkOrderNumber = $"أمر العمل رقم: {data.WorkOrderNumber}";
                    foreach (var item in data.RequiredMaterials)
                    {
                        ItemsToConsume.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل بيانات الصرف: {ex.Message}", "خطأ");
            }
        }

        /// <summary>
        /// يفتح نافذة اختيار الأرقام التسلسلية للمنتجات التي يتم تتبعها.
        /// </summary>
        private void SelectTrackingData(object parameter)
        {
            if (parameter is RequiredMaterialViewModel item)
            {
                if (item.ConsumedQuantity <= 0)
                {
                    MessageBox.Show("يرجى تحديد الكمية المراد صرفها أولاً.", "تنبيه");
                    return;
                }
                if (!item.SourceLocationId.HasValue)
                {
                    MessageBox.Show("يرجى تحديد الموقع المصدر أولاً.", "تنبيه");
                    return;
                }

                var selectionWindow = new SelectTrackingDataWindow(item.MaterialId, item.SourceLocationId.Value, (int)item.ConsumedQuantity, item.TrackingMethod);
                if (selectionWindow.ShowDialog() == true)
                {
                    item.SelectedSerialIds = selectionWindow.SelectedIds;
                    MessageBox.Show($"تم اختيار {item.SelectedSerialIds.Count} رقم بنجاح.", "نجاح");
                }
            }
        }

        /// <summary>
        /// شرط يحدد ما إذا كان يمكن تأكيد عملية الصرف.
        /// يجب ألا يكون هناك أي مادة تم تحديد كمية لصرفها بدون تحديد موقع الصرف.
        /// </summary>
        private bool CanConfirmConsumption(object parameter)
        {
            if (ItemsToConsume == null || !ItemsToConsume.Any()) return false;
            return !ItemsToConsume.Any(item => item.ConsumedQuantity > 0 && item.SourceLocationId == null);
        }

        /// <summary>
        /// يقوم بتنفيذ عملية الصرف النهائية عبر استدعاء الخدمة المختصة.
        /// </summary>
        private async void ConfirmConsumption(object parameter)
        {
            try
            {
                var itemsToProcess = ItemsToConsume.Where(i => i.ConsumedQuantity > 0).ToList();
                if (!itemsToProcess.Any())
                {
                    MessageBox.Show("لم يتم إدخال أي كميات للصرف.", "تنبيه");
                    return;
                }

                await _workOrderService.ConsumeMaterialsForWorkOrderAsync(_workOrderId, itemsToProcess);
                MessageBox.Show("تم تسجيل صرف المواد بنجاح.", "نجاح");

                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية الصرف: {ex.Message}", "خطأ");
            }
        }
    }
}