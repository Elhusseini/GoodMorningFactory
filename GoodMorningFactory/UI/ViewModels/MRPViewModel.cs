// UI/ViewModels/MRPViewModel.cs
// *** ملف جديد: ViewModel لواجهة تخطيط متطلبات المواد ***

using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class MRPViewModel : BaseViewModel
    {
        private readonly IMRPService _mrpService;

        public ObservableCollection<MRPResultViewModel> MrpResults { get; set; } = new ObservableCollection<MRPResultViewModel>();

        private bool _isProcessing;
        public bool IsProcessing { get => _isProcessing; set { _isProcessing = value; OnPropertyChanged(); } }

        public ICommand RunMrpCommand { get; }
        public ICommand CreateRequisitionCommand { get; }

        public MRPViewModel()
        {
            _mrpService = new MRPService();

            RunMrpCommand = new AsyncRelayCommand(RunMrpAsync);
            CreateRequisitionCommand = new RelayCommand(CreateRequisition);
        }

        private async Task RunMrpAsync()
        {
            IsProcessing = true;
            try
            {
                var results = await _mrpService.RunMRPAsync();
                MrpResults.Clear();
                foreach (var item in results)
                {
                    MrpResults.Add(item);
                }

                if (MrpResults.Count == 0)
                {
                    MessageBox.Show("لا توجد احتياجات حالية بناءً على أوامر البيع المفتوحة.", "اكتمل", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حساب المتطلبات: {ex.Message}", "خطأ فادح", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void CreateRequisition(object parameter)
        {
            if (parameter is MRPResultViewModel selectedMaterial)
            {
                if (selectedMaterial.NetRequirements <= 0) return;

                // استدعاء نافذة إنشاء طلب الشراء وتمرير البيانات اللازمة
                var requisitionWindow = new AddEditPurchaseRequisitionWindow(selectedMaterial.ProductId, selectedMaterial.NetRequirements);
                requisitionWindow.ShowDialog();

                // بعد إنشاء الطلب، من الجيد إعادة تشغيل الحساب لتحديث النتائج
                RunMrpCommand.Execute(null);
            }
        }
    }
}