// GoodMorningFactory/UI/Views/EnterTrackingDataWindow.xaml.cs
// *** الكود الكامل والنهائي - أصبح نظيفاً ومساعداً للـ ViewModel ***
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class EnterTrackingDataWindow : Window
    {
        // هذه الخصائص للقراءة فقط لجلب النتائج بعد إغلاق النافذة بنجاح
        public IEnumerable<string> SerialNumbers => (DataContext as EnterTrackingDataViewModel)?.SerialNumbers;
        public string LotNumber => (DataContext as EnterTrackingDataViewModel)?.LotNumber;
        public DateTime? ExpiryDate => (DataContext as EnterTrackingDataViewModel)?.ExpiryDate;

        public EnterTrackingDataWindow(ProductTrackingMethod trackingMethod, int requiredQuantity)
        {
            InitializeComponent();
            DataContext = new EnterTrackingDataViewModel(trackingMethod, requiredQuantity);
        }
    }
}