// GoodMorningFactory/UI/ViewModels/AddEditWorkOrderViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditWorkOrderViewModel : BaseViewModel
    {
        private readonly IWorkOrderService _workOrderService;
        private WorkOrder _workOrder;
        private bool _isNew = true;

        #region Properties
        private string _title;
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        private string _workOrderNumber;
        public string WorkOrderNumber { get => _workOrderNumber; set { _workOrderNumber = value; OnPropertyChanged(); } }

        public List<Product> FinishedGoods { get; private set; }

        private Product _selectedFinishedGood;
        public Product SelectedFinishedGood { get => _selectedFinishedGood; set { _selectedFinishedGood = value; OnPropertyChanged(); UpdateRequiredMaterials(); } }

        private int _quantityToProduce;
        public int QuantityToProduce { get => _quantityToProduce; set { _quantityToProduce = value; OnPropertyChanged(); UpdateRequiredMaterials(); } }

        private DateTime _plannedStartDate;
        public DateTime PlannedStartDate { get => _plannedStartDate; set { _plannedStartDate = value; OnPropertyChanged(); } }

        private DateTime _plannedEndDate;
        public DateTime PlannedEndDate { get => _plannedEndDate; set { _plannedEndDate = value; OnPropertyChanged(); } }

        private WorkOrderStatus _status;
        public WorkOrderStatus Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        public List<WorkOrderStatus> Statuses { get; private set; }
        public ObservableCollection<RequiredMaterialViewModel> RequiredMaterials { get; set; }
        public ObservableCollection<ConsumedMaterialViewModel> ConsumedMaterials { get; set; }

        private bool _isEditingAllowed;
        public bool IsEditingAllowed { get => _isEditingAllowed; set { _isEditingAllowed = value; OnPropertyChanged(); } }

        // --- بداية الإضافة: خصائص لعرض التكاليف ---
        private decimal _actualMaterialCost;
        public decimal ActualMaterialCost { get => _actualMaterialCost; set { _actualMaterialCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalActualCost)); } }

        private decimal _actualLaborCost;
        public decimal ActualLaborCost { get => _actualLaborCost; set { _actualLaborCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalActualCost)); } }

        public decimal TotalActualCost => ActualMaterialCost + ActualLaborCost;
        // --- نهاية الإضافة ---
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        // --- بداية الإضافة: أوامر الإجراءات ---
        public ICommand StartCommand { get; }
        public ICommand ConsumeMaterialsCommand { get; }
        public ICommand ReportProductionCommand { get; }
        public ICommand RecordLaborCommand { get; }
        public ICommand CloseWorkOrderCommand { get; }
        // --- نهاية الإضافة ---
        #endregion

        public AddEditWorkOrderViewModel() { /* For Design-Time */ }
        public AddEditWorkOrderViewModel(int? workOrderId = null, int? salesOrderItemId = null)
        {
            _workOrderService = new WorkOrderService();
            RequiredMaterials = new ObservableCollection<RequiredMaterialViewModel>();
            ConsumedMaterials = new ObservableCollection<ConsumedMaterialViewModel>();

            SaveCommand = new RelayCommand(Save, CanSave);
            // --- بداية الإضافة: ربط الأوامر بالدوال ---
            StartCommand = new RelayCommand(async _ => await UpdateStatus(WorkOrderStatus.InProgress), _ => CanStart());
            ConsumeMaterialsCommand = new RelayCommand(ConsumeMaterials, _ => CanPerformActions());
            ReportProductionCommand = new RelayCommand(ReportProduction, _ => CanPerformActions());
            RecordLaborCommand = new RelayCommand(RecordLabor, _ => CanPerformActions());
            CloseWorkOrderCommand = new RelayCommand(CloseWorkOrder, _ => CanClose());
            // --- نهاية الإضافة ---

            LoadInitialData(workOrderId, salesOrderItemId);
        }

        private async void LoadInitialData(int? workOrderId, int? salesOrderItemId)
        {
            try
            {
                var dto = await _workOrderService.GetInitialDataForAddEditWindowAsync(workOrderId, salesOrderItemId);
                _workOrder = dto.WorkOrder;

                FinishedGoods = dto.FinishedGoods;
                OnPropertyChanged(nameof(FinishedGoods));
                Statuses = dto.Statuses;
                OnPropertyChanged(nameof(Statuses));

                WorkOrderNumber = _workOrder.WorkOrderNumber;
                SelectedFinishedGood = FinishedGoods.FirstOrDefault(p => p.Id == _workOrder.FinishedGoodId);
                QuantityToProduce = _workOrder.QuantityToProduce;
                PlannedStartDate = _workOrder.PlannedStartDate;
                PlannedEndDate = _workOrder.PlannedEndDate;
                Status = _workOrder.Status;

                // --- بداية الإضافة: تحميل التكاليف الفعلية ---
                ActualLaborCost = _workOrder.TotalLaborCost;
                // (تكلفة المواد سيتم حسابها عند عرض التبويب)
                // --- نهاية الإضافة ---

                if (_workOrder.Id > 0)
                {
                    _isNew = false;
                    Title = "عرض / تعديل أمر عمل";
                    dto.ConsumedMaterials.ForEach(c => ConsumedMaterials.Add(c));
                    OnPropertyChanged(nameof(ConsumedMaterials));
                }
                else
                {
                    Title = "إنشاء أمر عمل جديد";
                }

                IsEditingAllowed = !(_workOrder.Status == WorkOrderStatus.Completed || _workOrder.Status == WorkOrderStatus.Cancelled);
                await UpdateRequiredMaterials();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        private async Task UpdateRequiredMaterials()
        {
            if (SelectedFinishedGood == null || QuantityToProduce <= 0)
            {
                RequiredMaterials.Clear();
                return;
            }
            try
            {
                var materials = await _workOrderService.GetRequiredMaterialsForProductAsync(SelectedFinishedGood.Id, QuantityToProduce);
                RequiredMaterials.Clear();
                materials.ForEach(m => RequiredMaterials.Add(m));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحديث المواد المطلوبة: {ex.Message}", "خطأ");
            }
        }
        private async void Save(object parameter)
        {
            _workOrder.FinishedGoodId = SelectedFinishedGood.Id;
            _workOrder.QuantityToProduce = QuantityToProduce;
            _workOrder.PlannedStartDate = PlannedStartDate;
            _workOrder.PlannedEndDate = PlannedEndDate;
            _workOrder.Status = Status;

            try
            {
                await _workOrderService.SaveWorkOrderAsync(_workOrder, _isNew);
                MessageBox.Show("تم حفظ أمر العمل بنجاح.", "نجاح");
                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ أمر العمل: {ex.Message}", "خطأ");
            }
        }
        private bool CanSave(object parameter) { return IsEditingAllowed && SelectedFinishedGood != null && QuantityToProduce > 0; }

        // --- بداية الإضافة: منطق الأوامر الجديدة ---
        private bool CanStart() => _workOrder?.Status == WorkOrderStatus.Planned;
        private bool CanPerformActions() => _workOrder?.Status == WorkOrderStatus.InProgress;
        private bool CanClose() => _workOrder?.Status == WorkOrderStatus.Completed;

        private async Task UpdateStatus(WorkOrderStatus newStatus)
        {
            try
            {
                await _workOrderService.UpdateWorkOrderStatusAsync(_workOrder.Id, newStatus);
                Status = newStatus; // تحديث الواجهة مباشرة
                MessageBox.Show("تم تحديث الحالة بنجاح.", "نجاح");
                IsEditingAllowed = !(Status == WorkOrderStatus.Completed || Status == WorkOrderStatus.Cancelled);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحديث الحالة: {ex.Message}", "خطأ");
            }
        }

        private void ConsumeMaterials(object obj)
        {
            var consumptionWindow = new MaterialConsumptionWindow(_workOrder.Id);
            consumptionWindow.ShowDialog();
            // إعادة تحميل البيانات لإظهار المواد المصروفة المحدثة
            LoadInitialData(_workOrder.Id, null);
        }

        private void ReportProduction(object obj)
        {
            var productionWindow = new ReportProductionWindow(_workOrder.Id);
            if (productionWindow.ShowDialog() == true)
            {
                LoadInitialData(_workOrder.Id, null); // إعادة تحميل لتحديث الحالة والكميات
            }
        }

        private void RecordLabor(object obj)
        {
            var laborWindow = new RecordLaborWindow(_workOrder.Id);
            if (laborWindow.ShowDialog() == true)
            {
                LoadInitialData(_workOrder.Id, null); // إعادة تحميل لتحديث تكلفة العمالة
            }
        }

        private async void CloseWorkOrder(object parameter)
        {
            var result = MessageBox.Show("هل أنت متأكد من رغبتك في إغلاق أمر العمل؟\nسيتم إنشاء قيد محاسبي وتحديث تكلفة المنتج. لا يمكن التراجع عن هذه العملية.", "تأكيد الإغلاق", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;

            try
            {
                await _workOrderService.CloseWorkOrderAsync(_workOrder.Id);
                MessageBox.Show("تم إغلاق أمر العمل بنجاح.", "نجاح");
                if (parameter is Window window)
                {
                    window.DialogResult = true; // لإعلام الواجهة الرئيسية بالتحديث
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل إغلاق أمر العمل: {ex.Message}", "خطأ فادح");
            }
        }
        // --- نهاية الإضافة ---
    }
}