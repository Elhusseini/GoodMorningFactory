// GoodMorningFactory/UI/ViewModels/PurchaseRequisitionsViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class PurchaseRequisitionsViewModel : BaseViewModel
    {
        private readonly IPurchaseRequisitionService _requisitionService;
        private int _currentPage = 1;
        private readonly int _pageSize = 15;
        private int _totalItems = 0;

        public ObservableCollection<PurchaseRequisition> Requisitions { get; set; }
        public List<object> StatusFilters { get; private set; }
        public object SelectedStatusFilter { get; set; }
        public string SearchText { get; set; }
        public string PageInfo { get; private set; }

        public RelayCommand SearchCommand { get; }
        public RelayCommand AddRequisitionCommand { get; }
        public RelayCommand EditRequisitionCommand { get; }
        public RelayCommand ConvertToPOCommand { get; }
        public RelayCommand NextPageCommand { get; }
        public RelayCommand PreviousPageCommand { get; }
        public RelayCommand SubmitForApprovalCommand { get; }

        public PurchaseRequisitionsViewModel()
        {
            _requisitionService = new PurchaseRequisitionService();
            SearchCommand = new RelayCommand(param => { _currentPage = 1; LoadRequisitions(); });
            AddRequisitionCommand = new RelayCommand(AddRequisition);
            EditRequisitionCommand = new RelayCommand(EditRequisition, CanEditRequisition);
            ConvertToPOCommand = new RelayCommand(ConvertToPO, CanConvertToPO);
            NextPageCommand = new RelayCommand(NextPage, CanGoNextPage);
            PreviousPageCommand = new RelayCommand(PreviousPage, CanGoPreviousPage);
            SubmitForApprovalCommand = new RelayCommand(SubmitForApproval, CanSubmitForApproval);

            LoadStatusFilters();
            LoadRequisitions();
        }

        private void LoadStatusFilters()
        {
            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(RequisitionStatus)).Cast<object>());
            StatusFilters = statuses;
            SelectedStatusFilter = StatusFilters.First();
        }

        public async void LoadRequisitions()
        {
            try
            {
                RequisitionStatus? status = null;
                if (SelectedStatusFilter is RequisitionStatus s) status = s;

                var result = await _requisitionService.GetRequisitionsAsync(_currentPage, _pageSize, SearchText, status);
                Requisitions = new ObservableCollection<PurchaseRequisition>(result.Items);
                _totalItems = result.TotalItems;
                OnPropertyChanged(nameof(Requisitions));
                UpdatePageInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل طلبات الشراء: {ex.Message}", "خطأ");
            }
        }

        private void UpdatePageInfo()
        {
            int totalPages = (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / _pageSize);
            PageInfo = $"الصفحة {_currentPage} من {totalPages} (إجمالي السجلات: {_totalItems})";
            OnPropertyChanged(nameof(PageInfo));
            NextPageCommand.RaiseCanExecuteChanged();
            PreviousPageCommand.RaiseCanExecuteChanged();
        }

        private void AddRequisition(object obj)
        {
            var addWindow = new AddEditPurchaseRequisitionWindow();
            if (addWindow.ShowDialog() == true) { LoadRequisitions(); }
        }

        private void EditRequisition(object obj)
        {
            if (obj is PurchaseRequisition requisition)
            {
                var editWindow = new AddEditPurchaseRequisitionWindow(requisition.Id);
                if (editWindow.ShowDialog() == true) { LoadRequisitions(); }
            }
        }
        private bool CanEditRequisition(object obj) => obj is PurchaseRequisition req && req.Status == RequisitionStatus.Draft;

        private async void SubmitForApproval(object obj)
        {
            if (obj is PurchaseRequisition requisition)
            {
                await _requisitionService.SubmitForApprovalAsync(requisition.Id);
                LoadRequisitions();
                MessageBox.Show("تم إرسال الطلب للموافقة بنجاح.", "نجاح");
            }
        }
        private bool CanSubmitForApproval(object obj) => obj is PurchaseRequisition req && req.Status == RequisitionStatus.Draft && PermissionsService.CanAccess("Purchasing.Requisitions.Submit");

        private void ConvertToPO(object obj)
        {
            if (obj is PurchaseRequisition requisition)
            {
                var poWindow = new AddEditPurchaseOrderWindow(sourceRequisitionId: requisition.Id);
                if (poWindow.ShowDialog() == true) { LoadRequisitions(); }
            }
        }
        private bool CanConvertToPO(object obj) => obj is PurchaseRequisition req && req.Status == RequisitionStatus.Approved && PermissionsService.CanAccess("Purchasing.Orders.Create");

        private void NextPage(object obj) { _currentPage++; LoadRequisitions(); }
        private bool CanGoNextPage(object obj) => _currentPage < (int)Math.Ceiling((double)_totalItems / _pageSize);

        private void PreviousPage(object obj) { _currentPage--; LoadRequisitions(); }
        private bool CanGoPreviousPage(object obj) => _currentPage > 1;
    }
}