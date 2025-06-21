// UI/ViewModels/BudgetDetailViewModel.cs
// *** ملف جديد: ViewModel لتمثيل سطر واحد في شاشة إدخال الموازنة ***
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class BudgetDetailViewModel : INotifyPropertyChanged
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }

        private decimal _month1;
        public decimal Month1 { get => _month1; set { _month1 = value; OnPropertyChanged(nameof(Month1)); } }

        private decimal _month2;
        public decimal Month2 { get => _month2; set { _month2 = value; OnPropertyChanged(nameof(Month2)); } }

        private decimal _month3;
        public decimal Month3 { get => _month3; set { _month3 = value; OnPropertyChanged(nameof(Month3)); } }

        private decimal _month4;
        public decimal Month4 { get => _month4; set { _month4 = value; OnPropertyChanged(nameof(Month4)); } }

        private decimal _month5;
        public decimal Month5 { get => _month5; set { _month5 = value; OnPropertyChanged(nameof(Month5)); } }

        private decimal _month6;
        public decimal Month6 { get => _month6; set { _month6 = value; OnPropertyChanged(nameof(Month6)); } }

        private decimal _month7;
        public decimal Month7 { get => _month7; set { _month7 = value; OnPropertyChanged(nameof(Month7)); } }

        private decimal _month8;
        public decimal Month8 { get => _month8; set { _month8 = value; OnPropertyChanged(nameof(Month8)); } }

        private decimal _month9;
        public decimal Month9 { get => _month9; set { _month9 = value; OnPropertyChanged(nameof(Month9)); } }

        private decimal _month10;
        public decimal Month10 { get => _month10; set { _month10 = value; OnPropertyChanged(nameof(Month10)); } }

        private decimal _month11;
        public decimal Month11 { get => _month11; set { _month11 = value; OnPropertyChanged(nameof(Month11)); } }

        private decimal _month12;
        public decimal Month12 { get => _month12; set { _month12 = value; OnPropertyChanged(nameof(Month12)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
