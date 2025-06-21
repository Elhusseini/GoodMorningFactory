// GoodMorningFactory/UI/Views/MaterialConsumptionWindow.xaml.cs
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// تم تحويل هذه النافذة بالكامل لتعمل بنمط MVVM.
    /// الكود الخلفي الآن مسؤول فقط عن إنشاء وتعيين الـ ViewModel كمصدر بيانات.
    /// </summary>
    public partial class MaterialConsumptionWindow : Window
    {
        public MaterialConsumptionWindow(int workOrderId)
        {
            InitializeComponent();
            // إنشاء الـ ViewModel وتمرير معرّف أمر العمل إليه، ثم تعيينه كمصدر بيانات
            DataContext = new MaterialConsumptionViewModel(workOrderId);
        }
    }
}