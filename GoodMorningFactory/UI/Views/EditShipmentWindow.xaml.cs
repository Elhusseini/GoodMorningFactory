// UI/Views/EditShipmentWindow.xaml.cs
using GoodMorningFactory.UI.ViewModels; // <-- إضافة using
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// تم تحويل هذه النافذة بالكامل لنمط MVVM.
    /// الكود الخلفي الآن مسؤول فقط عن إنشاء وتعيين الـ ViewModel.
    /// </summary>
    public partial class EditShipmentWindow : Window
    {
        public EditShipmentWindow(int shipmentId)
        {
            InitializeComponent();
            // إنشاء الـ ViewModel وتعيينه كمصدر بيانات للنافذة
            DataContext = new EditShipmentViewModel(shipmentId);
        }
    }
}