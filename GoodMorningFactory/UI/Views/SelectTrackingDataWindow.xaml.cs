// UI/Views/SelectTrackingDataWindow.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    // ViewModel بسيط لعرض بيانات التتبع في نافذة الاختيار
    public class TrackingDataSelectionViewModel
    {
        public long Id { get; set; }
        public string Value { get; set; }
        public System.DateTime? ExpiryDate { get; set; }
    }

    public partial class SelectTrackingDataWindow : Window
    {
        private readonly int _productId;
        private readonly int _locationId;
        private readonly int _requiredQuantity;
        private readonly ProductTrackingMethod _trackingMethod;

        public List<long> SelectedIds { get; private set; } = new List<long>();

        public SelectTrackingDataWindow(int productId, int locationId, int requiredQuantity, ProductTrackingMethod trackingMethod)
        {
            InitializeComponent();
            _productId = productId;
            _locationId = locationId;
            _requiredQuantity = requiredQuantity;
            _trackingMethod = trackingMethod;

            LoadAvailableData();
        }

        private void LoadAvailableData()
        {
            InstructionsTextBlock.Text = $"الكمية المطلوبة: {_requiredQuantity} | تم اختيار: 0";

            using (var db = new DatabaseContext())
            {
                if (_trackingMethod == ProductTrackingMethod.BySerialNumber)
                {
                    var availableSerials = db.SerialNumbers
                        .Where(sn => sn.ProductId == _productId && sn.StorageLocationId == _locationId && sn.Status == SerialNumberStatus.InStock)
                        .Select(sn => new TrackingDataSelectionViewModel { Id = sn.Id, Value = sn.Value })
                        .ToList();
                    TrackingDataListView.ItemsSource = availableSerials;
                }
                else // ByLotNumber
                {
                    // (منطق اختيار الدفعات يمكن إضافته هنا بنفس الطريقة)
                    MessageBox.Show("اختيار الدفعات لم يتم تنفيذه بعد.", "تحت التطوير");
                    this.Close();
                }
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrackingDataListView.SelectedItems.Count != _requiredQuantity)
            {
                MessageBox.Show($"يجب اختيار {_requiredQuantity} رقم تسلسلي بالضبط.", "عدد غير مطابق", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (TrackingDataSelectionViewModel selectedItem in TrackingDataListView.SelectedItems)
            {
                SelectedIds.Add(selectedItem.Id);
            }

            this.DialogResult = true;
        }
    }
}
