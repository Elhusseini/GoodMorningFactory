using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddShipmentViewModel : BaseViewModel
    {
        private readonly IShipmentService _shipmentService;
        private readonly int _salesOrderId;

        #region Properties
        private string _salesOrderNumber;
        public string SalesOrderNumber { get => _salesOrderNumber; set { _salesOrderNumber = value; OnPropertyChanged(); } }

        private DateTime _shipmentDate = DateTime.Today;
        public DateTime ShipmentDate { get => _shipmentDate; set { _shipmentDate = value; OnPropertyChanged(); } }

        public ObservableCollection<ShipmentItemViewModel> ItemsToShip { get; set; }
        #endregion

        #region Commands
        public ICommand ConfirmShipmentCommand { get; }
        public ICommand SelectTrackingDataCommand { get; }
        #endregion

        public AddShipmentViewModel()
        {
            // مُنشئ فارغ لوقت التصميم
            SalesOrderNumber = "SO-DESIGN-TIME";
            ItemsToShip = new ObservableCollection<ShipmentItemViewModel>
            {
                new ShipmentItemViewModel { ProductName = "منتج تصميم", OrderedQuantity = 10, PreviouslyShippedQuantity = 2, QuantityToShip = 8 }
            };
        }

        public AddShipmentViewModel(int salesOrderId)
        {
            _shipmentService = new ShipmentService();
            _salesOrderId = salesOrderId;
            ItemsToShip = new ObservableCollection<ShipmentItemViewModel>();

            ConfirmShipmentCommand = new RelayCommand(ConfirmShipment, CanConfirmShipment);
            SelectTrackingDataCommand = new RelayCommand(SelectTrackingData);

            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                var data = await _shipmentService.GetShipmentDataForCreationAsync(_salesOrderId);
                if (data != null)
                {
                    SalesOrderNumber = data.SalesOrderNumber;
                    ShipmentDate = data.ShipmentDate;
                    foreach (var item in data.ItemsToShip)
                    {
                        ItemsToShip.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل بيانات الشحنة: {ex.Message}", "خطأ");
            }
        }

        private void SelectTrackingData(object parameter)
        {
            if (parameter is ShipmentItemViewModel item)
            {
                if (item.QuantityToShip <= 0)
                {
                    MessageBox.Show("يرجى تحديد الكمية المراد شحنها أولاً.", "تنبيه");
                    return;
                }
                if (!item.SourceLocationId.HasValue)
                {
                    MessageBox.Show("يرجى تحديد الموقع المصدر أولاً.", "تنبيه");
                    return;
                }

                var selectionWindow = new SelectTrackingDataWindow(item.ProductId, item.SourceLocationId.Value, item.QuantityToShip, item.TrackingMethod);
                if (selectionWindow.ShowDialog() == true)
                {
                    item.SelectedSerialIds = selectionWindow.SelectedIds;
                    MessageBox.Show($"تم اختيار {item.SelectedSerialIds.Count} رقم بنجاح.", "نجاح");
                }
            }
        }

        private async void ConfirmShipment(object parameter)
        {
            try
            {
                await _shipmentService.CreateShipmentAndInvoiceAsync(_salesOrderId, ShipmentDate, ItemsToShip);
                MessageBox.Show("تم إنشاء الشحنة والفوترة بنجاح.", "نجاح");
                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ");
            }
        }

        private bool CanConfirmShipment(object parameter)
        {
            if (ItemsToShip == null || !ItemsToShip.Any()) return false;

            return !ItemsToShip.Any(item => item.QuantityToShip > 0 && item.SourceLocationId == null);
        }
    }
}
