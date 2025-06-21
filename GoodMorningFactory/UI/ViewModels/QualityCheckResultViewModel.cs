// UI/ViewModels/QualityCheckResultViewModel.cs
// *** ملف جديد: يمثل نتيجة فحص لمعيار واحد في الواجهة ***

using GoodMorningFactory.Data.Models;

namespace GoodMorningFactory.UI.ViewModels
{
    public class QualityCheckResultViewModel : BaseViewModel
    {
        private QualityCheckResult _result;
        public QualityCheckResult Result
        {
            get => _result;
            set { _result = value; OnPropertyChanged(); }
        }

        public QualityParameter Parameter { get; }

        public QualityCheckResultViewModel(QualityParameter parameter)
        {
            Parameter = parameter;
            Result = new QualityCheckResult
            {
                QualityParameterId = parameter.Id,
                IsConforming = true // القيمة الافتراضية
            };
        }
    }
}