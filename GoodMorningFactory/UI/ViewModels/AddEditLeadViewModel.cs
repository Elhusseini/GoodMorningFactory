// UI/ViewModels/AddEditLeadViewModel.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.Services;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Converters;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditLeadViewModel : INotifyPropertyChanged
    {
        private readonly ICrmService _crmService;

        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public ObservableCollection<object> Statuses { get; set; }
        public object SelectedStatus { get; set; }
        public ObservableCollection<User> Users { get; set; }
        public User SelectedUser { get; set; }

        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public AddEditLeadViewModel()
        {
            _crmService = new CrmService();
            Statuses = new ObservableCollection<object>();
            Users = new ObservableCollection<User>();

            LoadCommand = new RelayCommand(async _ => await LoadDataAsync());
            SaveCommand = new RelayCommand(async param => await SaveAsync(param as Window), _ => !string.IsNullOrWhiteSpace(CompanyName));

            LoadCommand.Execute(null);
        }

        private async Task LoadDataAsync()
        {
            var converter = new EnumToDescriptionConverter();
            var statuses = System.Enum.GetValues(typeof(LeadStatus))
                .Cast<LeadStatus>()
                .Select(e => new { Value = e, Description = converter.Convert(e, typeof(string), null, null) });

            foreach (var s in statuses) Statuses.Add(s);
            SelectedStatus = Statuses.FirstOrDefault();

            var users = await _crmService.GetActiveUsersAsync();
            foreach (var u in users) Users.Add(u);
        }

        private async Task SaveAsync(Window window)
        {
            var newLead = new Lead
            {
                CompanyName = this.CompanyName,
                ContactPerson = this.ContactPerson,
                Email = this.Email,
                PhoneNumber = this.PhoneNumber,
                Status = (LeadStatus)(SelectedStatus.GetType().GetProperty("Value")?.GetValue(SelectedStatus) ?? LeadStatus.New),
                AssignedToUserId = SelectedUser?.Id,
                Source = "Manual Entry",
                CreatedDate = System.DateTime.Now
            };

            try
            {
                await _crmService.AddLeadAsync(newLead);
                window.DialogResult = true;
                window.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"فشل حفظ العميل المحتمل: {ex.Message}", "خطأ");
            }
        }

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}