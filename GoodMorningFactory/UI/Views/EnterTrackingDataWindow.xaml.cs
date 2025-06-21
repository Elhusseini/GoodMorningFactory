// UI/Views/EnterTrackingDataWindow.xaml.cs
using GoodMorningFactory.Data.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public partial class EnterTrackingDataWindow : Window
    {
        private readonly ProductTrackingMethod _trackingMethod;
        private readonly int _requiredQuantity;

        public ObservableCollection<string> SerialNumbers { get; set; }
        public string LotNumber { get; private set; }
        public DateTime? ExpiryDate { get; private set; }

        public EnterTrackingDataWindow(ProductTrackingMethod trackingMethod, int requiredQuantity)
        {
            InitializeComponent();
            _trackingMethod = trackingMethod;
            _requiredQuantity = requiredQuantity;

            SerialNumbers = new ObservableCollection<string>();
            SerialNumbersListBox.ItemsSource = SerialNumbers;

            SetupUI();
        }

        private void SetupUI()
        {
            InstructionsTextBlock.Text = $"الكمية المطلوبة: {_requiredQuantity} | تم إدخال: {SerialNumbers.Count}";
            if (_trackingMethod == ProductTrackingMethod.BySerialNumber)
            {
                HeaderTextBlock.Text = "إدخال الأرقام التسلسلية";
                SerialEntryPanel.Visibility = Visibility.Visible;
                LotEntryPanel.Visibility = Visibility.Collapsed;
            }
            else // ByLotNumber
            {
                HeaderTextBlock.Text = "إدخال معلومات الدفعة";
                SerialEntryPanel.Visibility = Visibility.Collapsed;
                LotEntryPanel.Visibility = Visibility.Visible;
            }
        }

        private void SerialNumberTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var serial = SerialNumberTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(serial) && !SerialNumbers.Contains(serial))
                {
                    if (SerialNumbers.Count < _requiredQuantity)
                    {
                        SerialNumbers.Add(serial);
                        SerialNumberTextBox.Clear();
                        InstructionsTextBlock.Text = $"الكمية المطلوبة: {_requiredQuantity} | تم إدخال: {SerialNumbers.Count}";
                    }
                    else
                    {
                        MessageBox.Show("لقد قمت بإدخال العدد المطلوب من الأرقام التسلسلية.", "اكتمل العدد");
                    }
                }
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (_trackingMethod == ProductTrackingMethod.BySerialNumber)
            {
                if (SerialNumbers.Count != _requiredQuantity)
                {
                    MessageBox.Show($"يجب إدخال {_requiredQuantity} رقم تسلسلي.", "عدد غير مطابق", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else // ByLotNumber
            {
                if (string.IsNullOrWhiteSpace(LotNumberTextBox.Text))
                {
                    MessageBox.Show("يرجى إدخال رقم الدفعة.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                LotNumber = LotNumberTextBox.Text;
                ExpiryDate = ExpiryDatePicker.SelectedDate;
            }

            this.DialogResult = true;
        }
    }
}
