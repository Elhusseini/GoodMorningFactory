// GoodMorningFactory/UI/ViewModels/CrmViewModel.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.Services; // تأكد من أن هذا هو الـ namespace الصحيح للخدمة
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class CrmViewModel : INotifyPropertyChanged
    {
        private readonly ICrmService _crmService;
        private ObservableCollection<Lead> _leads;
        private ObservableCollection<Opportunity> _opportunities;

        public ObservableCollection<Lead> Leads
        {
            get => _leads;
            set { _leads = value; OnPropertyChanged(nameof(Leads)); }
        }

        public ObservableCollection<Opportunity> Opportunities
        {
            get => _opportunities;
            set { _opportunities = value; OnPropertyChanged(nameof(Opportunities)); }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand AddLeadCommand { get; }
        public ICommand ConvertLeadCommand { get; }
        public ICommand AddOpportunityCommand { get; }
        public ICommand EditOpportunityCommand { get; }
        public ICommand DeleteOpportunityCommand { get; }
        public ICommand CreateQuotationCommand { get; } // <-- إضافة جديدة

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public CrmViewModel()
        {
            _crmService = new CrmService();
            Leads = new ObservableCollection<Lead>();
            Opportunities = new ObservableCollection<Opportunity>();

            LoadDataCommand = new RelayCommand(async _ => await LoadDataAsync());
            AddLeadCommand = new RelayCommand(_ => AddNewLead());
            ConvertLeadCommand = new RelayCommand(async param => await ConvertLeadAsync(param as Lead), param => CanConvertLead(param as Lead));
            AddOpportunityCommand = new RelayCommand(_ => AddNewOpportunity());
            EditOpportunityCommand = new RelayCommand(param => EditOpportunity(param as Opportunity));
            DeleteOpportunityCommand = new RelayCommand(async param => await DeleteOpportunityAsync(param as Opportunity));
            CreateQuotationCommand = new RelayCommand(param => CreateQuotation(param as Opportunity)); // <-- إضافة جديدة

            LoadDataCommand.Execute(null);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var leadsList = await _crmService.GetLeadsAsync();
                Leads.Clear();
                foreach (var lead in leadsList) Leads.Add(lead);

                var opportunitiesList = await _crmService.GetOpportunitiesAsync();
                Opportunities.Clear();
                foreach (var opp in opportunitiesList) Opportunities.Add(opp);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ فادح");
            }
        }

        private void AddNewLead()
        {
            var addWindow = new AddEditLeadWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadDataCommand.Execute(null);
            }
        }

        private async Task ConvertLeadAsync(Lead leadToConvert)
        {
            var result = MessageBox.Show($"هل أنت متأكد من تحويل '{leadToConvert.CompanyName}' إلى عميل دائم؟", "تأكيد التحويل", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _crmService.ConvertLeadToCustomerAsync(leadToConvert);
                    MessageBox.Show("تم تحويل العميل بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDataCommand.Execute(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل تحويل العميل: {ex.Message}", "خطأ");
                }
            }
        }

        private bool CanConvertLead(Lead lead)
        {
            return lead != null && lead.Status == LeadStatus.Qualified;
        }

        private void AddNewOpportunity()
        {
            var addWindow = new AddEditOpportunityWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadDataCommand.Execute(null);
            }
        }

        private void EditOpportunity(Opportunity opportunityToEdit)
        {
            if (opportunityToEdit == null) return;

            var editWindow = new AddEditOpportunityWindow(opportunityToEdit);
            if (editWindow.ShowDialog() == true)
            {
                LoadDataCommand.Execute(null);
            }
        }

        private async Task DeleteOpportunityAsync(Opportunity opportunityToDelete)
        {
            if (opportunityToDelete == null) return;

            var result = MessageBox.Show($"هل أنت متأكد من حذف الفرصة البيعية: '{opportunityToDelete.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _crmService.DeleteOpportunityAsync(opportunityToDelete.Id);
                    MessageBox.Show("تم الحذف بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDataCommand.Execute(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل حذف الفرصة: {ex.Message}", "خطأ");
                }
            }
        }

        // --- بداية الإضافة: دالة إنشاء عرض السعر ---
        private void CreateQuotation(Opportunity sourceOpportunity)
        {
            if (sourceOpportunity == null) return;

            // فتح نافذة عرض السعر مع تمرير الفرصة كمصدر للبيانات
            var quotationWindow = new AddEditSalesQuotationWindow(sourceOpportunity);
            if (quotationWindow.ShowDialog() == true)
            {
                // إذا تم الحفظ بنجاح، قم بتحديث الفرصة في قاعدة البيانات
                // (هذه الخطوة يمكن تحسينها عبر إضافة دالة في الخدمة لتحديث الفرصة فقط)
                MessageBox.Show("تم إنشاء عرض السعر بنجاح. سيتم تحديث الفرصة.", "نجاح");
                Task.Run(async () => await LoadDataAsync()); // إعادة تحميل البيانات لإظهار التغيير
            }
        }
        // --- نهاية الإضافة ---
    }
}