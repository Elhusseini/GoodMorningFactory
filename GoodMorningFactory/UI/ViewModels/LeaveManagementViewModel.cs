// UI/ViewModels/LeaveManagementViewModel.cs
// *** ملف جديد: ViewModel لواجهة إدارة طلبات الإجازات ***

using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class LeaveManagementViewModel : BaseViewModel
    {
        private readonly IHRService _hrService;

        public ObservableCollection<LeaveRequest> LeaveRequests { get; set; } = new ObservableCollection<LeaveRequest>();

        private LeaveRequest _selectedRequest;
        public LeaveRequest SelectedRequest
        {
            get => _selectedRequest;
            set { _selectedRequest = value; OnPropertyChanged(); }
        }

        public ICommand LoadRequestsCommand { get; }
        public ICommand AddRequestCommand { get; }
        public ICommand ApproveRequestCommand { get; }
        public ICommand RejectRequestCommand { get; }

        public LeaveManagementViewModel()
        {
            _hrService = new HRService();

            LoadRequestsCommand = new AsyncRelayCommand(LoadRequestsAsync);
            AddRequestCommand = new RelayCommand(_ => AddRequest());
            ApproveRequestCommand = new AsyncRelayCommand(ApproveRequestAsync, () => SelectedRequest != null && SelectedRequest.Status == LeaveRequestStatus.Pending);
            RejectRequestCommand = new AsyncRelayCommand(RejectRequestAsync, () => SelectedRequest != null && SelectedRequest.Status == LeaveRequestStatus.Pending);

            LoadRequestsCommand.Execute(null);
        }

        private async Task LoadRequestsAsync()
        {
            var requests = await _hrService.GetLeaveRequestsAsync();
            LeaveRequests.Clear();
            foreach (var request in requests)
            {
                LeaveRequests.Add(request);
            }
        }

        private void AddRequest()
        {
            var addViewModel = new AddLeaveRequestViewModel(_hrService);
            var addWindow = new AddLeaveRequestWindow
            {
                DataContext = addViewModel
            };
            if (addWindow.ShowDialog() == true)
            {
                LoadRequestsCommand.Execute(null);
            }
        }

        private async Task UpdateRequestStatusAsync(LeaveRequestStatus newStatus, string actionName)
        {
            if (SelectedRequest == null) return;

            var result = MessageBox.Show($"هل أنت متأكد من {actionName} هذا الطلب؟", "تأكيد الإجراء", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            await _hrService.UpdateLeaveRequestStatusAsync(SelectedRequest.Id, newStatus);
            await LoadRequestsAsync(); // إعادة تحميل القائمة لتحديث الحالة
        }

        private async Task ApproveRequestAsync()
        {
            await UpdateRequestStatusAsync(LeaveRequestStatus.Approved, "الموافقة على");
        }

        private async Task RejectRequestAsync()
        {
            await UpdateRequestStatusAsync(LeaveRequestStatus.Rejected, "رفض");
        }
    }
}