// GoodMorningFactory/UI/ViewModels/EditShipmentViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using System;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel مخصص لنافذة تعديل بيانات الشحنة (شركة الشحن ورقم التتبع).
    /// </summary>
    public class EditShipmentViewModel : BaseViewModel
    {
        private readonly int _shipmentId;
        private readonly IShipmentService _shipmentService;

        #region الخصائص (Properties)
        private string _carrier;
        public string Carrier
        {
            get => _carrier;
            set { _carrier = value; OnPropertyChanged(); }
        }

        private string _trackingNumber;
        public string TrackingNumber
        {
            get => _trackingNumber;
            set { _trackingNumber = value; OnPropertyChanged(); }
        }
        #endregion

        #region الأوامر (Commands)
        public ICommand SaveCommand { get; }
        #endregion

        public EditShipmentViewModel(int shipmentId)
        {
            _shipmentId = shipmentId;
            _shipmentService = new ShipmentService();
            SaveCommand = new RelayCommand(SaveChanges, _ => !string.IsNullOrWhiteSpace(Carrier));

            LoadShipmentData();
        }

        private async void LoadShipmentData()
        {
            try
            {
                var shipment = await _shipmentService.GetShipmentForEditAsync(_shipmentId);
                if (shipment != null)
                {
                    Carrier = shipment.Carrier;
                    TrackingNumber = shipment.TrackingNumber;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل بيانات الشحنة: {ex.Message}", "خطأ");
            }
        }

        private async void SaveChanges(object parameter)
        {
            try
            {
                await _shipmentService.UpdateShipmentDetailsAsync(_shipmentId, Carrier, TrackingNumber);
                MessageBox.Show("تم حفظ التعديلات بنجاح.", "نجاح");

                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ التعديلات: {ex.Message}", "خطأ");
            }
        }
    }
}