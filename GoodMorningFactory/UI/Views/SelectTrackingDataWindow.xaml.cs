// UI/Views/SelectTrackingDataWindow.xaml.cs
// *** الكود الكامل والنهائي - أصبح نظيفاً ومساعداً للـ ViewModel ***
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class SelectTrackingDataWindow : Window
    {
        // هذه الخاصية للقراءة فقط لجلب النتائج بعد إغلاق النافذة بنجاح
        public List<long> SelectedIds => (DataContext as SelectTrackingDataViewModel)?.SelectedIds;

        public SelectTrackingDataWindow(int productId, int locationId, int requiredQuantity, ProductTrackingMethod trackingMethod)
        {
            InitializeComponent();
            DataContext = new SelectTrackingDataViewModel(productId, locationId, requiredQuantity, trackingMethod);
        }
    }
}