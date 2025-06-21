// GoodMorningFactory/UI/ViewModels/AddEditJournalVoucherViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditJournalVoucherViewModel : BaseViewModel
    {
        private readonly IJournalVoucherService _journalVoucherService;

        #region Properties
        public string VoucherNumber { get; set; }
        public DateTime VoucherDate { get; set; }
        public string Description { get; set; }
        public decimal TotalDebit => Items.Sum(i => i.Debit);
        public decimal TotalCredit => Items.Sum(i => i.Credit);

        // --- بداية الإضافة: خصائص جديدة لحالة التوازن ---
        public bool IsBalanced => TotalDebit > 0 && TotalDebit == TotalCredit;
        public string BalanceStatusText => IsBalanced ? "القيد متوازن" : "القيد غير متوازن";
        public Brush BalanceStatusColor => IsBalanced ? Brushes.Green : Brushes.Red;
        // --- نهاية الإضافة ---

        public ObservableCollection<JournalVoucherItemViewModel> Items { get; set; }
        public List<AccountViewModel> Accounts { get; private set; }
        public List<CostCenter> CostCenters { get; private set; }
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        // --- بداية الإضافة: أمر جديد لحذف سطر ---
        public ICommand DeleteItemCommand { get; }
        // --- نهاية الإضافة ---
        #endregion

        public AddEditJournalVoucherViewModel() { /* For Design-Time */ }

        public AddEditJournalVoucherViewModel(IJournalVoucherService journalVoucherService)
        {
            _journalVoucherService = journalVoucherService;
            Items = new ObservableCollection<JournalVoucherItemViewModel>();
            Items.CollectionChanged += (s, e) => { UpdateTotalsAndBalance(); };

            SaveCommand = new RelayCommand(Save, CanSave);
            // --- بداية الإضافة: ربط الأمر الجديد ---
            DeleteItemCommand = new RelayCommand(DeleteItem, CanDeleteItem);
            // --- نهاية الإضافة ---

            AddNewRow();
        }

        public async Task InitializeAsync()
        {
            var dto = await _journalVoucherService.GetInitialDataForAddEditWindowAsync();
            Accounts = dto.Accounts;
            CostCenters = dto.CostCenters;
            OnPropertyChanged(nameof(Accounts));
            OnPropertyChanged(nameof(CostCenters));

            VoucherNumber = $"JV-{DateTime.Now:yyyyMMddHHmmss}";
            VoucherDate = DateTime.Today;
            OnPropertyChanged(string.Empty);
        }

        private void AddNewRow()
        {
            var newItem = new JournalVoucherItemViewModel(this);
            newItem.PropertyChanged += Item_PropertyChanged;
            Items.Add(newItem);
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var currentItem = sender as JournalVoucherItemViewModel;

            // --- بداية التعديل: منطق إضافة سطر جديد ---
            // يتم إضافة سطر جديد فقط إذا كان هذا هو السطر الأخير وتم إدخال مبلغ فيه
            if ((e.PropertyName == nameof(JournalVoucherItemViewModel.Debit) || e.PropertyName == nameof(JournalVoucherItemViewModel.Credit))
                && Items.LastOrDefault() == currentItem
                && (currentItem.Debit > 0 || currentItem.Credit > 0))
            {
                AddNewRow();
            }
            // --- نهاية التعديل ---

            UpdateTotalsAndBalance();
        }

        // --- بداية الإضافة: دالة مركزية لتحديث الإجماليات ---
        private void UpdateTotalsAndBalance()
        {
            OnPropertyChanged(nameof(TotalDebit));
            OnPropertyChanged(nameof(TotalCredit));
            OnPropertyChanged(nameof(IsBalanced));
            OnPropertyChanged(nameof(BalanceStatusText));
            OnPropertyChanged(nameof(BalanceStatusColor));
        }
        // --- نهاية الإضافة ---

        // --- بداية الإضافة: منطق حذف سطر ---
        private void DeleteItem(object parameter)
        {
            if (parameter is JournalVoucherItemViewModel itemToDelete)
            {
                // إلغاء الاشتراك من الحدث لمنع تسرب الذاكرة
                itemToDelete.PropertyChanged -= Item_PropertyChanged;
                Items.Remove(itemToDelete);
            }
        }

        private bool CanDeleteItem(object parameter)
        {
            // لا يمكن حذف آخر سطر فارغ
            return Items.Count > 1;
        }
        // --- نهاية الإضافة ---

        private async void Save(object parameter)
        {
            var voucher = new JournalVoucher
            {
                VoucherNumber = this.VoucherNumber,
                VoucherDate = this.VoucherDate,
                Description = this.Description ?? string.Empty,
                TotalDebit = this.TotalDebit,
                TotalCredit = this.TotalCredit,
                Status = VoucherStatus.Posted
            };

            foreach (var item in Items.Where(i => i.AccountId > 0 && (i.Debit > 0 || i.Credit > 0)))
            {
                voucher.JournalVoucherItems.Add(new JournalVoucherItem
                {
                    AccountId = item.AccountId,
                    Debit = item.Debit,
                    Credit = item.Credit,
                    Description = item.Description ?? string.Empty,
                    CostCenterId = item.CostCenterId
                });
            }

            try
            {
                await _journalVoucherService.SaveVoucherAsync(voucher);
                MessageBox.Show("تم حفظ القيد اليومي بنجاح.", "نجاح");
                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"فشل حفظ القيد: {ex.Message}");
                if (ex.InnerException != null)
                {
                    sb.AppendLine("\n--- الخطأ الداخلي ---");
                    sb.AppendLine(ex.InnerException.ToString());
                }
                MessageBox.Show(sb.ToString(), "خطأ فادح");
            }
        }

        private bool CanSave(object parameter)
        {
            // الشرط الوحيد للحفظ هو أن يكون القيد متوازناً
            return IsBalanced;
        }
    }
}