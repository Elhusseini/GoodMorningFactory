// GoodMorningFactory/UI/ViewModels/EnterTrackingDataViewModel.cs
// *** ملف جديد: ViewModel لنافذة إدخال بيانات التتبع ***
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class EnterTrackingDataViewModel : BaseViewModel
    {
        private readonly ProductTrackingMethod _trackingMethod;
        private readonly int _requiredQuantity;

        #region Properties
        private string _headerText;
        public string HeaderText { get => _headerText; set { _headerText = value; OnPropertyChanged(); } }

        private string _instructionsText;
        public string InstructionsText { get => _instructionsText; set { _instructionsText = value; OnPropertyChanged(); } }

        private bool _isSerialEntryVisible;
        public bool IsSerialEntryVisible { get => _isSerialEntryVisible; set { _isSerialEntryVisible = value; OnPropertyChanged(); } }

        private bool _isLotEntryVisible;
        public bool IsLotEntryVisible { get => _isLotEntryVisible; set { _isLotEntryVisible = value; OnPropertyChanged(); } }

        // --- Serial Number Properties ---
        public ObservableCollection<string> SerialNumbers { get; } = new ObservableCollection<string>();

        private string _currentSerial;
        public string CurrentSerial { get => _currentSerial; set { _currentSerial = value; OnPropertyChanged(); } }

        // --- Lot Number Properties ---
        private string _lotNumber;
        public string LotNumber { get => _lotNumber; set { _lotNumber = value; OnPropertyChanged(); } }

        private DateTime? _expiryDate;
        public DateTime? ExpiryDate { get => _expiryDate; set { _expiryDate = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        public RelayCommand AddSerialCommand { get; }
        public RelayCommand ConfirmCommand { get; }
        #endregion

        public EnterTrackingDataViewModel(ProductTrackingMethod trackingMethod, int requiredQuantity)
        {
            _trackingMethod = trackingMethod;
            _requiredQuantity = requiredQuantity;

            AddSerialCommand = new RelayCommand(AddSerial, CanAddSerial);
            ConfirmCommand = new RelayCommand(Confirm, CanConfirm);

            SetupUI();
        }

        private void SetupUI()
        {
            UpdateInstructions();
            if (_trackingMethod == ProductTrackingMethod.BySerialNumber)
            {
                HeaderText = "إدخال الأرقام التسلسلية";
                IsSerialEntryVisible = true;
                IsLotEntryVisible = false;
            }
            else // ByLotNumber
            {
                HeaderText = "إدخال معلومات الدفعة";
                IsSerialEntryVisible = false;
                IsLotEntryVisible = true;
            }
        }

        private void UpdateInstructions()
        {
            InstructionsText = $"الكمية المطلوبة: {_requiredQuantity} | تم إدخال: {SerialNumbers.Count}";
        }

        private void AddSerial(object parameter)
        {
            var serialToAdd = CurrentSerial?.Trim();
            if (!string.IsNullOrEmpty(serialToAdd) && !SerialNumbers.Contains(serialToAdd))
            {
                SerialNumbers.Add(serialToAdd);
                CurrentSerial = string.Empty; // Clear the textbox
                UpdateInstructions();
                ConfirmCommand.RaiseCanExecuteChanged(); // Re-evaluate if the confirm button should be enabled
            }
        }

        private bool CanAddSerial(object parameter)
        {
            return SerialNumbers.Count < _requiredQuantity;
        }

        private void Confirm(object parameter)
        {
            if (parameter is Window window)
            {
                window.DialogResult = true;
            }
        }

        private bool CanConfirm(object parameter)
        {
            if (_trackingMethod == ProductTrackingMethod.BySerialNumber)
            {
                return SerialNumbers.Count == _requiredQuantity;
            }
            else // ByLotNumber
            {
                return !string.IsNullOrWhiteSpace(LotNumber);
            }
        }
    }
}