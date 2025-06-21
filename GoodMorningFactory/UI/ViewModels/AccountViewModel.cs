// UI/ViewModels/AccountViewModel.cs
// *** تحديث: تمت إضافة حقل الرصيد وخاصية العرض المنسق ***
using GoodMorningFactory.Data.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AccountViewModel : INotifyPropertyChanged
    {
        public Account Account { get; set; }
        public ObservableCollection<AccountViewModel> Children { get; set; }

        // --- بداية التحديث ---
        public decimal Balance { get; set; }

        // خاصية لعرض اسم الحساب مع رصيده بشكل منسق
        public string DisplayName => $"{Account.AccountName} [{Balance.ToString("C", new CultureInfo("ar-KW"))}]";
        // --- نهاية التحديث ---

        public AccountViewModel(Account account)
        {
            Account = account;
            Children = new ObservableCollection<AccountViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}