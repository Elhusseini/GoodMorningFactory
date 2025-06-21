// UI/ViewModels/BankReconciliationViewModel.cs
// *** ملف جديد: ViewModel لعرض حركات النظام للتسوية ***
using System;
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class BankReconciliationViewModel : INotifyPropertyChanged
    {
        public int JournalItemId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}