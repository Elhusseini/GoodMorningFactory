// UI/Views/AddEditJournalVoucherView.xaml.cs
// الكود الخلفي لنافذة إضافة وتعديل قيد يومي
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public class JournalVoucherItemViewModel : INotifyPropertyChanged
    {
        public int AccountId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Description { get; set; }
        public int? CostCenterId { get; set; } // ** إضافة جديدة **

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public partial class AddEditJournalVoucherView : Window
    {
        private ObservableCollection<JournalVoucherItemViewModel> _items = new ObservableCollection<JournalVoucherItemViewModel>();

        public AddEditJournalVoucherView()
        {
            InitializeComponent();
            LoadAccounts();
            VoucherDatePicker.SelectedDate = DateTime.Today;
            VoucherNumberTextBox.Text = $"JV-{DateTime.Now:yyyyMMddHHmmss}";
            VoucherItemsDataGrid.ItemsSource = _items;
        }

        private void LoadAccounts()
        {
            using (var db = new DatabaseContext())
            {
                var accounts = db.Accounts.OrderBy(a => a.AccountNumber).ToList();
                var accountColumn = (DataGridComboBoxColumn)VoucherItemsDataGrid.Columns[0];
                accountColumn.ItemsSource = accounts;
                var costCenters = db.CostCenters.Where(c => c.IsActive).ToList();
                var costCenterColumn = (DataGridComboBoxColumn)VoucherItemsDataGrid.Columns[4];
                costCenterColumn.ItemsSource = costCenters;
            }
        }

        private void UpdateTotals()
        {
            decimal totalDebit = _items.Sum(i => i.Debit);
            decimal totalCredit = _items.Sum(i => i.Credit);
            TotalDebitTextBlock.Text = totalDebit.ToString("N2");
            TotalCreditTextBlock.Text = totalCredit.ToString("N2");
        }

        private void VoucherItemsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => UpdateTotals()), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            decimal totalDebit = _items.Sum(i => i.Debit);
            decimal totalCredit = _items.Sum(i => i.Credit);

            if (totalDebit != totalCredit)
            {
                MessageBox.Show("القيد غير متوازن. يجب أن يكون إجمالي المدين مساوياً لإجمالي الدائن.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (totalDebit == 0)
            {
                MessageBox.Show("لا يمكن حفظ قيد فارغ.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // --- ** بداية الإضافة: التحقق من الفترة المحاسبية ** ---
            DateTime voucherDate = VoucherDatePicker.SelectedDate ?? DateTime.Today;
            using (var dbCheck = new DatabaseContext())
            {
                var period = dbCheck.AccountingPeriods.FirstOrDefault(p => p.Year == voucherDate.Year && p.Month == voucherDate.Month);
                if (period != null && period.Status == PeriodStatus.Closed)
                {
                    MessageBox.Show("لا يمكن تسجيل القيد في فترة محاسبية مغلقة.", "عملية مرفوضة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            // --- ** نهاية الإضافة ** ---
            try
            {
                using (var db = new DatabaseContext())
                {
                    var voucher = new JournalVoucher
                    {
                        VoucherNumber = VoucherNumberTextBox.Text,
                        VoucherDate = VoucherDatePicker.SelectedDate ?? DateTime.Today,
                        Description = DescriptionTextBox.Text,
                        TotalDebit = totalDebit,
                        TotalCredit = totalCredit
                    };

                    foreach (var item in _items.Where(i => i.AccountId > 0))
                    {
                        voucher.JournalVoucherItems.Add(new JournalVoucherItem
                        {
                            AccountId = item.AccountId,
                            Debit = item.Debit,
                            Credit = item.Credit,
                            Description = item.Description,
                            CostCenterId = item.CostCenterId // ** إضافة جديدة **

                        });
                    }

                    db.JournalVouchers.Add(voucher);
                    db.SaveChanges();
                    MessageBox.Show("تم حفظ القيد اليومي بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ القيد: {ex.InnerException?.Message ?? ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}