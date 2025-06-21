// GoodMorningFactory/UI/ViewModels/JournalVoucherItemViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices; // Required for CallerMemberName

namespace GoodMorningFactory.UI.ViewModels
{
    public class JournalVoucherItemViewModel : INotifyPropertyChanged
    {
        // --- بداية الإضافة: مرجع للـ ViewModel الأب ---
        public AddEditJournalVoucherViewModel ParentViewModel { get; }
        // --- نهاية الإضافة ---

        private int _accountId;
        public int AccountId
        {
            get => _accountId;
            set { SetProperty(ref _accountId, value); }
        }

        private decimal _debit;
        public decimal Debit
        {
            get => _debit;
            set { SetProperty(ref _debit, value); }
        }

        private decimal _credit;
        public decimal Credit
        {
            get => _credit;
            set { SetProperty(ref _credit, value); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { SetProperty(ref _description, value); }
        }

        private int? _costCenterId;
        public int? CostCenterId
        {
            get => _costCenterId;
            set { SetProperty(ref _costCenterId, value); }
        }

        // --- بداية الإضافة: Constructor جديد ---
        public JournalVoucherItemViewModel(AddEditJournalVoucherViewModel parent)
        {
            ParentViewModel = parent;
        }
        // --- نهاية الإضافة ---

        public event PropertyChangedEventHandler PropertyChanged;

        // --- بداية التحسين: دالة مساعدة لتجنب التكرار ---
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // --- نهاية التحسين ---
    }
}