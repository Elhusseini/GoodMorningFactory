// UI/ViewModels/LeaveTypesViewModel.cs
// *** ملف جديد: ViewModel لواجهة إدارة أنواع الإجازات ***

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
    public class LeaveTypesViewModel : BaseViewModel
    {
        private readonly IHRService _hrService;

        public ObservableCollection<LeaveType> LeaveTypes { get; set; } = new ObservableCollection<LeaveType>();

        private LeaveType _selectedLeaveType;
        public LeaveType SelectedLeaveType
        {
            get => _selectedLeaveType;
            set { _selectedLeaveType = value; OnPropertyChanged(); }
        }

        public ICommand LoadLeaveTypesCommand { get; }
        public ICommand AddLeaveTypeCommand { get; }
        public ICommand EditLeaveTypeCommand { get; }
        public ICommand DeleteLeaveTypeCommand { get; }

        public LeaveTypesViewModel()
        {
            _hrService = new HRService();

            LoadLeaveTypesCommand = new AsyncRelayCommand(LoadLeaveTypesAsync);
            AddLeaveTypeCommand = new RelayCommand(_ => AddLeaveType());
            EditLeaveTypeCommand = new RelayCommand(async _ => await EditLeaveType(), _ => SelectedLeaveType != null);
            DeleteLeaveTypeCommand = new AsyncRelayCommand(DeleteLeaveTypeAsync, () => SelectedLeaveType != null);

            LoadLeaveTypesCommand.Execute(null);
        }

        private async Task LoadLeaveTypesAsync()
        {
            var types = await _hrService.GetLeaveTypesAsync();
            LeaveTypes.Clear();
            foreach (var type in types)
            {
                LeaveTypes.Add(type);
            }
        }

        private void AddLeaveType()
        {
            var addViewModel = new AddEditLeaveTypeViewModel(_hrService);
            var addWindow = new AddEditLeaveTypeWindow
            {
                DataContext = addViewModel
            };
            if (addWindow.ShowDialog() == true)
            {
                LoadLeaveTypesCommand.Execute(null);
            }
        }

        private async Task EditLeaveType()
        {
            if (SelectedLeaveType == null) return;
            var typeToEdit = await _hrService.GetLeaveTypeByIdAsync(SelectedLeaveType.Id);

            var editViewModel = new AddEditLeaveTypeViewModel(_hrService, typeToEdit);
            var editWindow = new AddEditLeaveTypeWindow
            {
                DataContext = editViewModel
            };
            if (editWindow.ShowDialog() == true)
            {
                LoadLeaveTypesCommand.Execute(null);
            }
        }

        private async Task DeleteLeaveTypeAsync()
        {
            if (SelectedLeaveType == null) return;
            var result = MessageBox.Show($"هل أنت متأكد من حذف '{SelectedLeaveType.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                await _hrService.DeleteLeaveTypeAsync(SelectedLeaveType.Id);
                LoadLeaveTypesCommand.Execute(null);
            }
        }
    }
}