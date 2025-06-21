// GoodMorningFactory/UI/ViewModels/WorkOrdersViewModel.cs
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
    public class WorkOrdersViewModel : BaseViewModel
    {
        private readonly IWorkOrderService _workOrderService;
        private bool _isInitialized = false;

        #region Properties
        private ObservableCollection<WorkOrderViewModel> _workOrders;
        public ObservableCollection<WorkOrderViewModel> WorkOrders
        {
            get => _workOrders;
            set { _workOrders = value; OnPropertyChanged(); }
        }

        public List<object> StatusFilters { get; private set; }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ResetAndLoad(); } }

        private object _selectedStatus;
        public object SelectedStatus { get => _selectedStatus; set { _selectedStatus = value; OnPropertyChanged(); ResetAndLoad(); } }

        private string _pageInfo;
        public string PageInfo { get => _pageInfo; set { _pageInfo = value; OnPropertyChanged(); } }
        private int _currentPage = 1;
        private readonly int _pageSize = 15;
        private int _totalItems = 0;
        #endregion

        #region Commands
        public ICommand LoadDataCommand { get; } // <-- الأمر الجديد
        public ICommand AddWorkOrderCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand ConsumeMaterialsCommand { get; }
        public ICommand ReportProductionCommand { get; }
        public ICommand RecordLaborCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        #endregion

        public WorkOrdersViewModel()
        {
            bool isInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());

            if (!isInDesignMode)
            {
                _workOrderService = new WorkOrderService();
            }

            WorkOrders = new ObservableCollection<WorkOrderViewModel>();

            LoadDataCommand = new RelayCommand(async _ => await InitializeAsync());
            AddWorkOrderCommand = new RelayCommand(ExecuteAddWorkOrder);
            ViewDetailsCommand = new RelayCommand(ExecuteViewDetails);
            StartCommand = new RelayCommand(ExecuteStart, CanExecuteAction);
            ConsumeMaterialsCommand = new RelayCommand(ExecuteConsumeMaterials, CanExecuteAction);
            ReportProductionCommand = new RelayCommand(ExecuteReportProduction, CanExecuteAction);
            RecordLaborCommand = new RelayCommand(ExecuteRecordLabor, CanExecuteAction);
            CancelCommand = new RelayCommand(ExecuteCancel, CanExecuteAction);
            NextPageCommand = new RelayCommand(GoToNextPage);
            PreviousPageCommand = new RelayCommand(GoToPreviousPage);
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized) return;

            StatusFilters = await _workOrderService.GetStatusFiltersAsync();
            _selectedStatus = StatusFilters.FirstOrDefault();
            OnPropertyChanged(nameof(StatusFilters));
            OnPropertyChanged(nameof(SelectedStatus));

            await LoadWorkOrdersAsync();
            _isInitialized = true;
        }

        private async Task LoadWorkOrdersAsync()
        {
            if (_workOrderService == null) return;
            try
            {
                var criteria = new WorkOrderFilterCriteria
                {
                    Page = _currentPage,
                    PageSize = _pageSize,
                    SearchText = SearchText,
                    Status = (SelectedStatus is WorkOrderStatus status) ? status : (WorkOrderStatus?)null
                };

                var result = await _workOrderService.GetWorkOrdersAsync(criteria);
                var viewModels = result.Items.Select(wo => new WorkOrderViewModel
                {
                    Id = wo.Id,
                    WorkOrderNumber = wo.WorkOrderNumber,
                    FinishedGoodName = wo.FinishedGood.Name,
                    QuantityToProduce = wo.QuantityToProduce,
                    QuantityProduced = wo.QuantityProduced,
                    PlannedStartDate = wo.PlannedStartDate,
                    Status = wo.Status
                }).ToList();

                WorkOrders = new ObservableCollection<WorkOrderViewModel>(viewModels);
                _totalItems = result.TotalCount;
                UpdatePageInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل أوامر العمل: {ex.Message}", "خطأ");
            }
        }

        private void ExecuteAddWorkOrder(object obj)
        {
            var addWindow = new AddEditWorkOrderWindow();
            if (addWindow.ShowDialog() == true) ResetAndLoad();
        }

        private void ExecuteViewDetails(object parameter)
        {
            if (parameter is WorkOrderViewModel vm)
            {
                var editWindow = new AddEditWorkOrderWindow(vm.Id);
                if (editWindow.ShowDialog() == true) LoadWorkOrdersAsync();
            }
        }

        private async void ExecuteStart(object parameter)
        {
            if (parameter is WorkOrderViewModel vm)
                await UpdateStatus(vm.Id, WorkOrderStatus.InProgress);
        }

        private void ExecuteConsumeMaterials(object parameter)
        {
            if (parameter is WorkOrderViewModel vm)
            {
                var consumptionWindow = new MaterialConsumptionWindow(vm.Id);
                consumptionWindow.ShowDialog();
            }
        }

        private void ExecuteReportProduction(object parameter)
        {
            if (parameter is WorkOrderViewModel vm)
            {
                var productionWindow = new ReportProductionWindow(vm.Id);
                if (productionWindow.ShowDialog() == true) LoadWorkOrdersAsync();
            }
        }

        private void ExecuteRecordLabor(object parameter)
        {
            if (parameter is WorkOrderViewModel vm)
            {
                var laborWindow = new RecordLaborWindow(vm.Id);
                laborWindow.ShowDialog();
            }
        }

        private async void ExecuteCancel(object parameter)
        {
            if (parameter is WorkOrderViewModel vm)
            {
                var result = MessageBox.Show($"هل أنت متأكد من إلغاء أمر العمل؟ سيتم إلغاء حجز أي مواد متبقية.", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    await UpdateStatus(vm.Id, WorkOrderStatus.Cancelled);
                }
            }
        }

        private bool CanExecuteAction(object parameter) => parameter is WorkOrderViewModel;

        private async Task UpdateStatus(int orderId, WorkOrderStatus status)
        {
            try
            {
                await _workOrderService.UpdateWorkOrderStatusAsync(orderId, status);
                await LoadWorkOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت عملية تحديث الحالة: {ex.Message}", "خطأ");
            }
        }

        private async void ResetAndLoad()
        {
            if (!_isInitialized) return;
            _currentPage = 1;
            await LoadWorkOrdersAsync();
        }

        private async void GoToNextPage(object p) { if (_currentPage < GetTotalPages()) { _currentPage++; await LoadWorkOrdersAsync(); } }
        private async void GoToPreviousPage(object p) { if (_currentPage > 1) { _currentPage--; await LoadWorkOrdersAsync(); } }
        private int GetTotalPages() => (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / _pageSize);
        private void UpdatePageInfo() => PageInfo = $"الصفحة {_currentPage} من {GetTotalPages()} (إجمالي الأوامر: {_totalItems})";
    }
}