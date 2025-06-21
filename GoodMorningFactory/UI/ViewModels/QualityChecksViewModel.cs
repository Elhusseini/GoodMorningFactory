// UI/ViewModels/QualityChecksViewModel.cs
// *** ملف جديد: ViewModel لواجهة عرض عمليات الفحص ***

using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class QualityChecksViewModel : BaseViewModel
    {
        private readonly IQualityService _qualityService;
        private readonly IHRService _hrService; // مطلوب لتمريره للنافذة الجديدة

        public ObservableCollection<QualityCheck> QualityChecks { get; set; } = new ObservableCollection<QualityCheck>();
        public QualityCheck SelectedQualityCheck { get; set; }

        public ICommand LoadChecksCommand { get; }
        public ICommand AddCheckCommand { get; }
        public ICommand ViewCheckCommand { get; }

        public QualityChecksViewModel()
        {
            _qualityService = new QualityService();
            _hrService = new HRService();

            LoadChecksCommand = new AsyncRelayCommand(LoadChecksAsync);
            AddCheckCommand = new RelayCommand(_ => AddCheck());
            ViewCheckCommand = new RelayCommand(async _ => await ViewCheck(), _ => SelectedQualityCheck != null);

            LoadChecksCommand.Execute(null);
        }

        private async Task LoadChecksAsync()
        {
            var checks = await _qualityService.GetQualityChecksAsync();
            QualityChecks.Clear();
            foreach (var check in checks)
            {
                QualityChecks.Add(check);
            }
        }

        private void AddCheck()
        {
            var addViewModel = new AddEditQualityCheckViewModel(_qualityService, _hrService);
            var addWindow = new AddEditQualityCheckWindow { DataContext = addViewModel };
            if (addWindow.ShowDialog() == true)
            {
                LoadChecksCommand.Execute(null);
            }
        }

        private async Task ViewCheck()
        {
            // للتعديل والعرض، سنستخدم نفس النافذة ولكن سنمرر لها البيانات
            var checkToView = await _qualityService.GetQualityCheckByIdAsync(SelectedQualityCheck.Id);
            // ... سيتم تطوير هذا الجزء لاحقاً
        }
    }
}