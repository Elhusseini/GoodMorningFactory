using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ManagePermissionsViewModel : BaseViewModel
    {
        private readonly IRoleService _roleService;
        private readonly int _roleId;
        private readonly bool _isReadOnly;

        #region Properties
        private string _roleNameText;
        public string RoleNameText
        {
            get => _roleNameText;
            set { _roleNameText = value; OnPropertyChanged(); }
        }

        private ObservableCollection<PermissionGroupViewModel> _permissionGroups;
        public ObservableCollection<PermissionGroupViewModel> PermissionGroups
        {
            get => _permissionGroups;
            set { _permissionGroups = value; OnPropertyChanged(); }
        }

        private List<Role> _rolesForCopy;
        public List<Role> RolesForCopy
        {
            get => _rolesForCopy;
            set { _rolesForCopy = value; OnPropertyChanged(); }
        }

        private Role _selectedRoleToCopy;
        public Role SelectedRoleToCopy
        {
            get => _selectedRoleToCopy;
            set { _selectedRoleToCopy = value; OnPropertyChanged(); }
        }

        // خصائص للتحكم في الواجهة
        public bool IsEditingEnabled => !_isReadOnly;
        public Visibility ControlsVisibility => _isReadOnly ? Visibility.Collapsed : Visibility.Visible;
        public string CancelButtonText => _isReadOnly ? "إغلاق" : "إلغاء";
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        public ICommand CopyCommand { get; }
        #endregion

        /// <summary>
        /// مُنشئ فارغ مخصص لمحرر XAML فقط لتجنب أخطاء وقت التصميم.
        /// </summary>
        public ManagePermissionsViewModel()
        {
            // هذا الكود يعمل فقط في وضع التصميم
            RoleNameText = "صلاحيات الدور: [اسم الدور]";
            PermissionGroups = new ObservableCollection<PermissionGroupViewModel>();
        }

        public ManagePermissionsViewModel(int roleId, bool isReadOnly = false)
        {
            _roleService = new RoleService();
            _roleId = roleId;
            _isReadOnly = isReadOnly;

            SaveCommand = new RelayCommand(SavePermissions, _ => !_isReadOnly);
            CopyCommand = new RelayCommand(CopyPermissions, _ => !_isReadOnly && SelectedRoleToCopy != null);

            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(_roleId);
                if (role == null) return;
                RoleNameText = $"صلاحيات الدور: {role.Name}";

                PermissionGroups = await _roleService.GetPermissionsForRoleAsync(_roleId);
                if (!_isReadOnly)
                {
                    RolesForCopy = await _roleService.GetRolesForCopyingAsync(_roleId);
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل تحميل الصلاحيات: {ex.Message}", "خطأ"); }
        }

        private async void SavePermissions(object parameter)
        {
            try
            {
                await _roleService.SavePermissionsForRoleAsync(_roleId, PermissionGroups);
                MessageBox.Show("تم حفظ الصلاحيات بنجاح.", "نجاح");
            }
            catch (Exception ex) { MessageBox.Show($"فشل حفظ الصلاحيات: {ex.Message}", "خطأ"); }
        }

        private async void CopyPermissions(object parameter)
        {
            var result = MessageBox.Show("سيتم استبدال الصلاحيات المحددة حالياً بصلاحيات الدور المختار. هل أنت متأكد؟", "تأكيد النسخ", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            try
            {
                var sourcePermissions = await _roleService.GetPermissionsForRoleAsync(SelectedRoleToCopy.Id);
                var sourcePermissionIds = sourcePermissions.SelectMany(g => g.Permissions)
                                                           .Where(p => p.IsSelected)
                                                           .Select(p => p.Id)
                                                           .ToHashSet();

                foreach (var group in PermissionGroups)
                {
                    foreach (var perm in group.Permissions)
                    {
                        perm.IsSelected = sourcePermissionIds.Contains(perm.Id);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"فشل نسخ الصلاحيات: {ex.Message}", "خطأ"); }
        }
    }
}
