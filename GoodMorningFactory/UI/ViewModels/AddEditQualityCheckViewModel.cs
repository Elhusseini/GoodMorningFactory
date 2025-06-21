// UI/ViewModels/AddEditQualityCheckViewModel.cs
// *** ملف جديد: ViewModel لنافذة إضافة وتعديل عملية فحص ***

using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditQualityCheckViewModel : BaseViewModel
    {
        private readonly IQualityService _qualityService;
        private readonly IHRService _hrService; // قد نحتاجه لجلب المنتجات

        private QualityCheck _qualityCheck;
        public QualityCheck QualityCheck
        {
            get => _qualityCheck;
            set { _qualityCheck = value; OnPropertyChanged(); }
        }

        public string WindowTitle { get; private set; }

        // قوائم للـ ComboBoxes
        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();
        public ObservableCollection<QualityCheckType> CheckTypes { get; } = new ObservableCollection<QualityCheckType>(Enum.GetValues(typeof(QualityCheckType)).Cast<QualityCheckType>());
        public ObservableCollection<QualityStatus> Statuses { get; } = new ObservableCollection<QualityStatus>(Enum.GetValues(typeof(QualityStatus)).Cast<QualityStatus>());

        // قائمة نتائج الفحص التي سيتم عرضها في الجدول
        public ObservableCollection<QualityCheckResultViewModel> Results { get; set; } = new ObservableCollection<QualityCheckResultViewModel>();

        public ICommand SaveCommand { get; }

        public AddEditQualityCheckViewModel(IQualityService qualityService, IHRService hrService)
        {
            _qualityService = qualityService;
            _hrService = hrService; // افترضنا أن المنتجات يمكن جلبها عبر خدمة أخرى أو يمكن تعديلها

            QualityCheck = new QualityCheck { CheckDate = DateTime.Now, OverallStatus = QualityStatus.Pending };
            WindowTitle = "تسجيل عملية فحص جديدة";

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            _ = LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                // ملاحظة: ستحتاج إلى دالة لجلب المنتجات. سنفترض أنها موجودة في IHRService كمثال.
                // من الأفضل إنشاء IProductService لهذا الغرض.
                // var productsList = await _productService.GetProductsAsync();
                // Products.Clear();
                // foreach(var p in productsList) { Products.Add(p); }

                var parameters = await _qualityService.GetQualityParametersAsync();
                Results.Clear();
                foreach (var param in parameters)
                {
                    Results.Add(new QualityCheckResultViewModel(param));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات الأولية: {ex.Message}");
            }
        }

        private async Task SaveAsync()
        {
            if (QualityCheck.ProductId == 0)
            {
                MessageBox.Show("يرجى اختيار المنتج.", "بيانات ناقصة");
                return;
            }

            // تجميع النتائج من الـ ViewModel
            QualityCheck.Results.Clear();
            foreach (var resultVM in Results)
            {
                QualityCheck.Results.Add(resultVM.Result);
            }

            await _qualityService.SaveQualityCheckAsync(QualityCheck);
        }
    }
}