// GoodMorningFactory/UI/Views/ReportProductionWindow.xaml.cs
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// تم تحويل هذه النافذة بالكامل لتعمل بنمط MVVM.
    /// الكود الخلفي الآن مسؤول فقط عن إنشاء وتعيين الـ ViewModel.
    /// </summary>
    public partial class ReportProductionWindow : Window
    {
        public ReportProductionWindow(int workOrderId)
        {
            InitializeComponent();
            // إنشاء الـ ViewModel وتمرير معرّف أمر العمل إليه
            DataContext = new ReportProductionViewModel(workOrderId);
        }
    }
}