// UI/ViewModels/AddEditQualityParameterViewModel.cs
// *** تحديث: تمت إضافة مُنشئ افتراضي لإصلاح خطأ مصمم XAML ***

using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditQualityParameterViewModel : BaseViewModel
    {
        private readonly IQualityService _qualityService;

        // الخاصية التي سيتم ربطها بحقول الإدخال في الواجهة
        private QualityParameter _parameter;
        public QualityParameter Parameter
        {
            get => _parameter;
            set { _parameter = value; OnPropertyChanged(); }
        }

        // خاصية لعنوان النافذة (إضافة أو تعديل)
        public string WindowTitle { get; private set; }

        // قائمة بأنواع المعايير لملء القائمة المنسدلة
        public List<ParameterType> ParameterTypes { get; } = Enum.GetValues(typeof(ParameterType)).Cast<ParameterType>().ToList();

        // أمر الحفظ
        public ICommand SaveCommand { get; }

        // *** بداية الإضافة: مُنشئ افتراضي لمصمم XAML ***
        /// <summary>
        /// هذا المنشئ يستخدمه مصمم الواجهات في Visual Studio فقط.
        /// يقوم بإنشاء بيانات وهمية لعرضها أثناء التصميم.
        /// </summary>
        public AddEditQualityParameterViewModel()
        {
            // هذا الكود يعمل فقط في وضع التصميم
            Parameter = new QualityParameter { Name = "اسم المعيار", ExpectedValue = "القيمة المتوقعة" };
            WindowTitle = "إضافة / تعديل (وضع التصميم)";
        }
        // *** نهاية الإضافة ***

        // المنشئ الخاص بحالة "الإضافة" الذي يعمل عند تشغيل البرنامج
        public AddEditQualityParameterViewModel(IQualityService qualityService)
        {
            _qualityService = qualityService;
            Parameter = new QualityParameter();
            WindowTitle = "إضافة معيار فحص جديد";
            SaveCommand = new AsyncRelayCommand(SaveAsync);
        }

        // المنشئ الخاص بحالة "التعديل" الذي يعمل عند تشغيل البرنامج
        public AddEditQualityParameterViewModel(IQualityService qualityService, QualityParameter parameterToEdit)
        {
            _qualityService = qualityService;
            Parameter = parameterToEdit;
            WindowTitle = $"تعديل معيار: {Parameter.Name}";
            SaveCommand = new AsyncRelayCommand(SaveAsync);
        }

        /// <summary>
        /// دالة تقوم بحفظ البيانات عبر استدعاء الخدمة.
        /// </summary>
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Parameter.Name))
            {
                MessageBox.Show("اسم المعيار حقل مطلوب.", "بيانات ناقصة");
                return;
            }

            try
            {
                await _qualityService.SaveQualityParameterAsync(Parameter);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ المعيار: {ex.Message}", "خطأ");
            }
        }
    }
}