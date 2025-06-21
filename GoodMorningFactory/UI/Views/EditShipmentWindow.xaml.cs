// UI/Views/EditShipmentWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة تعديل الشحنة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class EditShipmentWindow : Window
    {
        private readonly int _shipmentId;

        public EditShipmentWindow(int shipmentId)
        {
            InitializeComponent();
            _shipmentId = shipmentId;
            LoadShipmentData();
        }

        private void LoadShipmentData()
        {
            using (var db = new DatabaseContext())
            {
                var shipment = db.Shipments.Find(_shipmentId);
                if (shipment != null)
                {
                    CarrierTextBox.Text = shipment.Carrier;
                    TrackingNumberTextBox.Text = shipment.TrackingNumber;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var shipment = db.Shipments.Find(_shipmentId);
                    if (shipment != null)
                    {
                        shipment.Carrier = CarrierTextBox.Text;
                        shipment.TrackingNumber = TrackingNumberTextBox.Text;
                        db.SaveChanges();
                        MessageBox.Show("تم حفظ التعديلات بنجاح.", "نجاح");
                        this.DialogResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ التعديلات: {ex.Message}", "خطأ");
            }
        }
    }
}