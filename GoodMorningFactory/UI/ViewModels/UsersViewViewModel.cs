using GoodMorningFactory.Core.Services;
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
    public class UsersViewViewModel : BaseViewModel
    {
        private readonly IUserService _userService;

        #region Properties
        private ObservableCollection<UserViewModel> _users;
        public ObservableCollection<UserViewModel> Users
        {
            get => _users;
            set { _users = value; OnPropertyChanged(); }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ResetAndLoad(); }
        }

        public ObservableCollection<FilterItem<bool?>> StatusFilters { get; }
        private FilterItem<bool?> _selectedStatusFilter;
        public FilterItem<bool?> SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set { _selectedStatusFilter = value; OnPropertyChanged(); ResetAndLoad(); }
        }

        private string _pageInfo;
        public string PageInfo
        {
            get => _pageInfo;
            set { _pageInfo = value; OnPropertyChanged(); }
        }

        private int _currentPage = 1;
        private readonly int _pageSize = 10;
        private int _totalItems = 0;
        #endregion

        #region Commands
        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand ToggleStatusCommand { get; }
        public ICommand ViewPermissionsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        #endregion

        public UsersViewViewModel()
        {
            _userService = new UserService();
            StatusFilters = new ObservableCollection<FilterItem<bool?>>(new FilterService().GetStatusFilters());
            _selectedStatusFilter = StatusFilters.First();

            AddUserCommand = new RelayCommand(AddUser);
            EditUserCommand = new RelayCommand(EditUser, CanActOnUser);
            ToggleStatusCommand = new RelayCommand(ToggleStatus, CanActOnUser);
            ViewPermissionsCommand = new RelayCommand(ViewPermissions, CanActOnUser);
            RefreshCommand = new RelayCommand(async _ => await LoadUsersAsync());
            NextPageCommand = new RelayCommand(GoToNextPage, _ => _currentPage < GetTotalPages());
            PreviousPageCommand = new RelayCommand(GoToPreviousPage, _ => _currentPage > 1);

            LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                var criteria = new UserFilterCriteria
                {
                    Page = _currentPage,
                    PageSize = _pageSize,
                    SearchText = this.SearchText,
                    IsActive = this.SelectedStatusFilter?.Value
                };
                var result = await _userService.GetUsersAsync(criteria);
                Users = new ObservableCollection<UserViewModel>(result.Items);
                _totalItems = result.TotalCount;
                UpdatePageInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المستخدمين: {ex.Message}", "خطأ");
            }
        }

        private void AddUser(object parameter)
        {
            var addWindow = new AddEditUserWindow();
            if (addWindow.ShowDialog() == true) ResetAndLoad();
        }

        private async void EditUser(object parameter)
        {
            if (parameter is UserViewModel userVm)
            {
                var userToEdit = await _userService.GetUserByIdAsync(userVm.Id);
                if (userToEdit != null)
                {
                    var editWindow = new AddEditUserWindow(userToEdit);
                    if (editWindow.ShowDialog() == true) LoadUsersAsync();
                }
            }
        }

        private async void ToggleStatus(object parameter)
        {
            if (parameter is UserViewModel userVm)
            {
                string action = userVm.IsActive ? "تعطيل" : "تفعيل";
                var result = MessageBox.Show($"هل أنت متأكد من {action} حساب المستخدم '{userVm.Username}'؟", "تأكيد الإجراء", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _userService.ToggleUserStatusAsync(userVm.Id);
                        await LoadUsersAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "عملية غير مسموحة");
                    }
                }
            }
        }

        private void ViewPermissions(object parameter)
        {
            if (parameter is UserViewModel userVm)
            {
                var permissionsWindow = new ManagePermissionsWindow(userVm.RoleId, true);
                permissionsWindow.ShowDialog();
            }
        }

        private bool CanActOnUser(object parameter) => parameter is UserViewModel;

        #region Pagination
        private async void ResetAndLoad()
        {
            _currentPage = 1;
            await LoadUsersAsync();
        }
        private async void GoToNextPage(object parameter) { if (_currentPage < GetTotalPages()) { _currentPage++; await LoadUsersAsync(); } }
        private async void GoToPreviousPage(object parameter) { if (_currentPage > 1) { _currentPage--; await LoadUsersAsync(); } }
        private int GetTotalPages() => (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / _pageSize);
        private void UpdatePageInfo() => PageInfo = $"الصفحة {_currentPage} من {GetTotalPages()} (إجمالي المستخدمين: {_totalItems})";
        #endregion
    }
}
