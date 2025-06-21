// GoodMorningFactory/UI/Views/RecordLaborWindow.xaml.cs
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// تم تحويل هذه النافذة بالكامل لتعمل بنمط MVVM.
    /// الكود الخلفي الآن مسؤول فقط عن إنشاء وتعيين الـ ViewModel.
    /// </summary>
    public partial class RecordLaborWindow : Window
    {
        public RecordLaborWindow(int workOrderId)
        {
            InitializeComponent();
            // إنشاء الـ ViewModel وتمرير معرّف أمر العمل إليه
            DataContext = new RecordLaborViewModel(workOrderId);
        }
    }
}