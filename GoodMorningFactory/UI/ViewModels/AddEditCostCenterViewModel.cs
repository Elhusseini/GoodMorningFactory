// GoodMorningFactory/UI/ViewModels/AddEditCostCenterViewModel.cs
// *** الكود الكامل والمؤكد ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditCostCenterViewModel : BaseViewModel
    {
        private readonly ICostCenterService _costCenterService;
        private CostCenter _costCenter;

        public CostCenter CostCenter
        {
            get => _costCenter;
            set { _costCenter = value; OnPropertyChanged(); }
        }

        public string WindowTitle { get; private set; }
        public RelayCommand SaveCommand { get; }

        public AddEditCostCenterViewModel(ICostCenterService service, int? costCenterId)
        {
            _costCenterService = service;
            SaveCommand = new RelayCommand(async (p) => await SaveAsync(p as Window));
            LoadData(costCenterId);
        }

        private async void LoadData(int? costCenterId)
        {
            if (costCenterId.HasValue && costCenterId != 0)
            {
                WindowTitle = "تعديل مركز تكلفة";
                CostCenter = await _costCenterService.GetCostCenterByIdAsync(costCenterId.Value);
            }
            else
            {
                WindowTitle = "إضافة مركز تكلفة جديد";
                // بما أن النموذج الآن يحتوي على قيمة افتراضية، فالكود هنا أصبح آمنًا
                CostCenter = new CostCenter();
            }
            OnPropertyChanged(nameof(WindowTitle));
        }

        private async Task SaveAsync(Window window)
        {
            if (string.IsNullOrWhiteSpace(CostCenter.Name))
            {
                MessageBox.Show("اسم مركز التكلفة حقل مطلوب.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _costCenterService.SaveCostCenterAsync(CostCenter);
                window.DialogResult = true;
            }
            catch (Exception ex)
            {
                // عرض رسالة خطأ أكثر تفصيلاً للمساعدة في التشخيص
                MessageBox.Show($"فشل حفظ مركز التكلفة: {ex.Message}\n\nInner Exception: {ex.InnerException?.Message}", "خطأ");
            }
        }
    }
}