using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class RolesViewViewModel : BaseViewModel
    {
        private readonly IRoleService _roleService;

        #region Properties
        private ObservableCollection<RoleViewModel> _roles;
        public ObservableCollection<RoleViewModel> Roles
        {
            get => _roles;
            set { _roles = value; OnPropertyChanged(); }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ResetAndLoad(); }
        }

        private string _pageInfo;
        public string PageInfo
        {
            get => _pageInfo;
            set { _pageInfo = value; OnPropertyChanged(); }
        }

        private int _currentPage = 1;
        private readonly int _pageSize = 15;
        private int _totalItems = 0;
        #endregion

        #region Commands
        public ICommand AddRoleCommand { get; }
        public ICommand EditRoleCommand { get; }
        public ICommand DeleteRoleCommand { get; }
        public ICommand ManagePermissionsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        #endregion

        public RolesViewViewModel()
        {
            _roleService = new RoleService();
            AddRoleCommand = new RelayCommand(AddRole);
            EditRoleCommand = new RelayCommand(EditRole, CanActOnRole);
            DeleteRoleCommand = new RelayCommand(DeleteRole, CanActOnRole);
            ManagePermissionsCommand = new RelayCommand(ManagePermissions, CanActOnRole);
            RefreshCommand = new RelayCommand(async _ => await LoadRolesAsync());
            NextPageCommand = new RelayCommand(GoToNextPage, _ => _currentPage < GetTotalPages());
            PreviousPageCommand = new RelayCommand(GoToPreviousPage, _ => _currentPage > 1);
            LoadRolesAsync();
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                var result = await _roleService.GetRolesAsync(SearchText, _currentPage, _pageSize);
                Roles = new ObservableCollection<RoleViewModel>(result.Items);
                _totalItems = result.TotalCount;
                UpdatePageInfo();
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل الأدوار: {ex.Message}", "خطأ"); }
        }

        private void AddRole(object parameter)
        {
            var addWindow = new AddEditRoleWindow();
            if (addWindow.ShowDialog() == true) ResetAndLoad();
        }

        private void EditRole(object parameter)
        {
            if (parameter is RoleViewModel role)
            {
                var editWindow = new AddEditRoleWindow(role.Id);
                if (editWindow.ShowDialog() == true) LoadRolesAsync();
            }
        }

        private void ManagePermissions(object parameter)
        {
            if (parameter is RoleViewModel role)
            {
                var permissionsWindow = new ManagePermissionsWindow(role.Id);
                permissionsWindow.ShowDialog();
            }
        }

        private async void DeleteRole(object parameter)
        {
            if (parameter is RoleViewModel role)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف الدور '{role.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;
                try
                {
                    await _roleService.DeleteRoleAsync(role.Id);
                    await LoadRolesAsync();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ في الحذف"); }
            }
        }

        private bool CanActOnRole(object parameter) => parameter is RoleViewModel;

        #region Pagination
        private async void ResetAndLoad() { _currentPage = 1; await LoadRolesAsync(); }
        private async void GoToNextPage(object p) { if (_currentPage < GetTotalPages()) { _currentPage++; await LoadRolesAsync(); } }
        private async void GoToPreviousPage(object p) { if (_currentPage > 1) { _currentPage--; await LoadRolesAsync(); } }
        private int GetTotalPages() => (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / _pageSize);
        private void UpdatePageInfo() => PageInfo = $"الصفحة {_currentPage} من {GetTotalPages()} (إجمالي الأدوار: {_totalItems})";
        #endregion
    }
}
