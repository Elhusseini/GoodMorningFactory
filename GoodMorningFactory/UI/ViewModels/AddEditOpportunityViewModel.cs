// GoodMorningFactory/UI/ViewModels/AddEditOpportunityViewModel.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.Services;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Converters;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditOpportunityViewModel : INotifyPropertyChanged
    {
        private readonly ICrmService _crmService;
        private Opportunity _opportunity;

        public Opportunity Opportunity
        {
            get => _opportunity;
            set { _opportunity = value; OnPropertyChanged(nameof(Opportunity)); }
        }

        public ObservableCollection<Customer> Customers { get; set; }
        public ObservableCollection<User> Users { get; set; }
        public ObservableCollection<object> Stages { get; set; }

        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public AddEditOpportunityViewModel(Opportunity opportunity = null)
        {
            _crmService = new CrmService();
            Opportunity = opportunity ?? new Opportunity { CloseDate = DateTime.Now };

            Customers = new ObservableCollection<Customer>();
            Users = new ObservableCollection<User>();
            Stages = new ObservableCollection<object>();

            LoadCommand = new RelayCommand(async _ => await LoadDataAsync());
            SaveCommand = new RelayCommand(async param => await SaveAsync(param as Window), _ => CanSave());

            LoadCommand.Execute(null);
        }

        private async Task LoadDataAsync()
        {
            // جلب العملاء
            var customersList = await _crmService.GetActiveCustomersAsync();
            foreach (var c in customersList) Customers.Add(c);

            // جلب المستخدمين
            var usersList = await _crmService.GetActiveUsersAsync();
            foreach (var u in usersList) Users.Add(u);

            // تعبئة قائمة المراحل
            var converter = new EnumToDescriptionConverter();
            var stagesList = Enum.GetValues(typeof(OpportunityStage))
                .Cast<OpportunityStage>()
                .Select(e => new { Value = e, Description = converter.Convert(e, typeof(string), null, null) });
            foreach (var s in stagesList) Stages.Add(s);

            // تحديد القيم الافتراضية إذا كانت فرصة جديدة
            if (Opportunity.Id == 0)
            {
                Opportunity.Stage = OpportunityStage.Qualification;
            }
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Opportunity.Name) && Opportunity.CustomerId > 0;
        }

        private async Task SaveAsync(Window window)
        {
            try
            {
                await _crmService.SaveOpportunityAsync(Opportunity);
                window.DialogResult = true;
                window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ الفرصة البيعية: {ex.Message}", "خطأ");
            }
        }

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}