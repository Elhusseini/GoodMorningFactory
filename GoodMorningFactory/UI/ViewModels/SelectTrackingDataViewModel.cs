// GoodMorningFactory/UI/ViewModels/SelectTrackingDataViewModel.cs
// *** ملف جديد: ViewModel لنافذة اختيار بيانات التتبع ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GoodMorningFactory.UI.ViewModels
{
    public class TrackingDataSelectionItemViewModel : BaseViewModel
    {
        public long Id { get; set; }
        public string Value { get; set; }
        public DateTime? ExpiryDate { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }
    }

    public class SelectTrackingDataViewModel : BaseViewModel
    {
        private readonly int _productId;
        private readonly int _locationId;
        private readonly int _requiredQuantity;
        private readonly ProductTrackingMethod _trackingMethod;

        #region Properties
        public ObservableCollection<TrackingDataSelectionItemViewModel> AvailableItems { get; } = new ObservableCollection<TrackingDataSelectionItemViewModel>();

        private string _headerText;
        public string HeaderText { get => _headerText; set { _headerText = value; OnPropertyChanged(); } }

        private string _instructionsText;
        public string InstructionsText { get => _instructionsText; set { _instructionsText = value; OnPropertyChanged(); } }

        public ICollectionView ItemsView { get; }
        #endregion

        #region Commands
        public RelayCommand ConfirmCommand { get; }
        #endregion

        public List<long> SelectedIds { get; } = new List<long>();

        public SelectTrackingDataViewModel(int productId, int locationId, int requiredQuantity, ProductTrackingMethod trackingMethod)
        {
            _productId = productId;
            _locationId = locationId;
            _requiredQuantity = requiredQuantity;
            _trackingMethod = trackingMethod;

            ConfirmCommand = new RelayCommand(Confirm, CanConfirm);

            // إعداد فلتر وعرض البيانات
            ItemsView = CollectionViewSource.GetDefaultView(AvailableItems);
            LoadAvailableDataAsync();
        }

        private async void LoadAvailableDataAsync()
        {
            using (var db = new DatabaseContext())
            {
                if (_trackingMethod == ProductTrackingMethod.BySerialNumber)
                {
                    HeaderText = "اختيار الأرقام التسلسلية";
                    var availableSerials = await db.SerialNumbers
                        .Where(sn => sn.ProductId == _productId && sn.StorageLocationId == _locationId && sn.Status == SerialNumberStatus.InStock)
                        .Select(sn => new TrackingDataSelectionItemViewModel { Id = sn.Id, Value = sn.Value })
                        .ToListAsync();

                    AvailableItems.Clear();
                    foreach (var serial in availableSerials)
                    {
                        serial.PropertyChanged += (s, e) => UpdateSelectionCount();
                        AvailableItems.Add(serial);
                    }
                }
                else
                {
                    HeaderText = "اختيار الدفعات";
                    // ... منطق جلب الدفعات يضاف هنا ...
                }
            }
            UpdateSelectionCount();
        }

        private void UpdateSelectionCount()
        {
            int selectedCount = AvailableItems.Count(i => i.IsSelected);
            InstructionsText = $"الكمية المطلوبة: {_requiredQuantity} | تم اختيار: {selectedCount}";
            ConfirmCommand.RaiseCanExecuteChanged();
        }

        private bool CanConfirm(object parameter)
        {
            return AvailableItems.Count(i => i.IsSelected) == _requiredQuantity;
        }

        private void Confirm(object parameter)
        {
            SelectedIds.Clear();
            SelectedIds.AddRange(AvailableItems.Where(i => i.IsSelected).Select(i => i.Id));

            if (parameter is Window window)
            {
                window.DialogResult = true;
            }
        }
    }
}