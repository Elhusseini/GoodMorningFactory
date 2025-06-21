// GoodMorningFactory/UI/ViewModels/AddEditRoleViewModel.cs
// *** A new file for the Add/Edit Role window's ViewModel ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditRoleViewModel : BaseViewModel
    {
        private readonly IRoleService _roleService;
        private readonly int? _roleId;
        private Role _role;

        public Role Role
        {
            get => _role;
            set { _role = value; OnPropertyChanged(); }
        }

        public string WindowTitle { get; private set; }
        public RelayCommand SaveCommand { get; }

        public AddEditRoleViewModel(int? roleId)
        {
            _roleId = roleId;
            _roleService = new RoleService(); // Using your existing service
            SaveCommand = new RelayCommand(async (p) => await SaveAsync(p as Window));
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            if (_roleId.HasValue)
            {
                WindowTitle = "تعديل دور";
                Role = await _roleService.GetRoleByIdAsync(_roleId.Value);
            }
            else
            {
                WindowTitle = "إضافة دور جديد";
                Role = new Role();
            }
            OnPropertyChanged(nameof(WindowTitle));
        }

        private async Task SaveAsync(Window window)
        {
            if (string.IsNullOrWhiteSpace(Role.Name))
            {
                MessageBox.Show("اسم الدور حقل مطلوب.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_roleId.HasValue)
                {
                    await _roleService.UpdateRoleAsync(Role);
                }
                else
                {
                    await _roleService.AddRoleAsync(Role);
                }
                window.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ الدور: {ex.Message}", "خطأ");
            }
        }
    }
}