using GoodMorningFactory.Data.Models;
using System.Collections.ObjectModel;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel يمثل عقدة واحدة (حساب واحد) في شجرة الحسابات.
    /// تم تحديثه ليتوافق مع الكود الأصلي مع إضافة تحسينات MVVM.
    /// </summary>
    public class AccountViewModel : BaseViewModel
    {
        /// <summary>
        /// الموديل الأصلي للحساب.
        /// </summary>
        public Account Model { get; }

        public int Id => Model.Id;
        public string DisplayName => $"{Model.AccountNumber} - {Model.AccountName}";
        public ObservableCollection<AccountViewModel> Children { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public AccountViewModel(Account account)
        {
            Model = account;
            Children = new ObservableCollection<AccountViewModel>();
        }
    }
}
