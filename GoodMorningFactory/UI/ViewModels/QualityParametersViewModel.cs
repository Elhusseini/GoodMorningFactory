// UI/ViewModels/QualityParametersViewModel.cs
// *** ملف جديد: ViewModel لواجهة إدارة معايير الفحص ***

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
    public class QualityParametersViewModel : BaseViewModel
    {
        private readonly IQualityService _qualityService;

        // قائمة المعايير التي سيتم عرضها في الجدول
        public ObservableCollection<QualityParameter> Parameters { get; set; } = new ObservableCollection<QualityParameter>();

        // المعيار المحدد حالياً في الجدول
        private QualityParameter _selectedParameter;
        public QualityParameter SelectedParameter
        {
            get => _selectedParameter;
            set { _selectedParameter = value; OnPropertyChanged(); }
        }

        // الأوامر الخاصة بالواجهة
        public ICommand LoadParametersCommand { get; }
        public ICommand AddParameterCommand { get; }
        public ICommand EditParameterCommand { get; }
        public ICommand DeleteParameterCommand { get; }

        public QualityParametersViewModel()
        {
            _qualityService = new QualityService();

            // ربط الأوامر بالدوال الخاصة بها
            LoadParametersCommand = new AsyncRelayCommand(LoadParametersAsync);
            AddParameterCommand = new RelayCommand(_ => AddParameter());
            EditParameterCommand = new RelayCommand(async _ => await EditParameter(), _ => SelectedParameter != null);
            DeleteParameterCommand = new AsyncRelayCommand(DeleteParameterAsync, () => SelectedParameter != null);

            // تحميل البيانات عند بدء تشغيل الواجهة
            LoadParametersCommand.Execute(null);
        }

        /// <summary>
        /// تحميل قائمة معايير الفحص من قاعدة البيانات.
        /// </summary>
        private async Task LoadParametersAsync()
        {
            var parametersList = await _qualityService.GetQualityParametersAsync();
            Parameters.Clear();
            foreach (var param in parametersList)
            {
                Parameters.Add(param);
            }
        }

        /// <summary>
        /// فتح نافذة إضافة معيار جديد.
        /// </summary>
        private void AddParameter()
        {
            var addViewModel = new AddEditQualityParameterViewModel(_qualityService);
            var addWindow = new AddEditQualityParameterWindow
            {
                DataContext = addViewModel
            };
            if (addWindow.ShowDialog() == true)
            {
                LoadParametersCommand.Execute(null); // تحديث القائمة بعد الإضافة
            }
        }

        /// <summary>
        /// فتح نافذة تعديل المعيار المحدد.
        /// </summary>
        private async Task EditParameter()
        {
            if (SelectedParameter == null) return;
            var parameterToEdit = await _qualityService.GetQualityParameterByIdAsync(SelectedParameter.Id);

            var editViewModel = new AddEditQualityParameterViewModel(_qualityService, parameterToEdit);
            var editWindow = new AddEditQualityParameterWindow
            {
                DataContext = editViewModel
            };
            if (editWindow.ShowDialog() == true)
            {
                LoadParametersCommand.Execute(null); // تحديث القائمة بعد التعديل
            }
        }

        /// <summary>
        /// حذف المعيار المحدد بعد تأكيد المستخدم.
        /// </summary>
        private async Task DeleteParameterAsync()
        {
            if (SelectedParameter == null) return;
            var result = MessageBox.Show($"هل أنت متأكد من حذف المعيار '{SelectedParameter.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                await _qualityService.DeleteQualityParameterAsync(SelectedParameter.Id);
                LoadParametersCommand.Execute(null); // تحديث القائمة بعد الحذف
            }
        }
    }
}