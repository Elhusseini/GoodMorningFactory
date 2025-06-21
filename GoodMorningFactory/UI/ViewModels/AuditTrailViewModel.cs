// GoodMorningFactory/UI/ViewModels/AuditTrailViewModel.cs
// *** الكود الكامل والنهائي بعد إصلاح خطأ تعريف الأمر ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AuditTrailViewModel : BaseViewModel
    {
        private readonly IAuditTrailService _auditService;

        #region Properties
        public ObservableCollection<AuditLog> AuditLogs { get; } = new ObservableCollection<AuditLog>();
        public ObservableCollection<string> UserFilterOptions { get; } = new ObservableCollection<string>();

        private DateTime? _fromDate;
        public DateTime? FromDate { get => _fromDate; set { _fromDate = value; OnPropertyChanged(); } }

        private DateTime? _toDate;
        public DateTime? ToDate { get => _toDate; set { _toDate = value; OnPropertyChanged(); } }

        private string _selectedUser;
        public string SelectedUser { get => _selectedUser; set { _selectedUser = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }

        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }
        #endregion

        public RelayCommand SearchCommand { get; }

        public AuditTrailViewModel()
        {
            _auditService = new AuditTrailService();
            // ======================= بداية الإصلاح الرئيسي =======================
            // تم تعديل تعريف الأمر ليقبل باراميتر (param) كما يتطلب RelayCommand
            SearchCommand = new RelayCommand(async (param) => await LoadLogsAsync());
            // ======================== نهاية الإصلاح الرئيسي ========================
            Initialize();
        }

        private async void Initialize()
        {
            await LoadUserFilterOptionsAsync();
            await LoadLogsAsync();
        }

        private async Task LoadUserFilterOptionsAsync()
        {
            try
            {
                var users = await _auditService.GetUsernamesForFilterAsync();
                UserFilterOptions.Clear();
                foreach (var user in users)
                {
                    UserFilterOptions.Add(user);
                }
                SelectedUser = UserFilterOptions[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل قائمة المستخدمين: {ex.Message}", "خطأ");
            }
        }

        private async Task LoadLogsAsync()
        {
            IsLoading = true;
            try
            {
                var criteria = new AuditFilterCriteria
                {
                    FromDate = this.FromDate,
                    ToDate = this.ToDate,
                    SelectedUser = this.SelectedUser,
                    SearchText = this.SearchText
                };

                var logs = await _auditService.GetAuditLogsAsync(criteria);
                AuditLogs.Clear();
                foreach (var log in logs)
                {
                    AuditLogs.Add(log);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل السجلات: {ex.Message}", "خطأ");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}