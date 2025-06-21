using GoodMorningFactory.Data.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AccountViewModel : INotifyPropertyChanged
    {
        public Account Model { get; }

        public int Id => Model.Id;
        public string AccountNumber => Model.AccountNumber;
        public string AccountName => Model.AccountName;
        public string DisplayName => $"{AccountNumber} - {AccountName}";

        public ObservableCollection<AccountViewModel> Children { get; set; }

        public AccountViewModel(Account account)
        {
            Model = account;
            Children = new ObservableCollection<AccountViewModel>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
