// UI/Views/AddEditBudgetWindow.xaml.cs
// *** ملف جديد: الكود الخلفي لنافذة إضافة وتعديل تفاصيل الموازنة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditBudgetWindow : Window
    {
        private readonly int? _budgetId;
        private ObservableCollection<BudgetDetailViewModel> _budgetItems = new ObservableCollection<BudgetDetailViewModel>();

        public AddEditBudgetWindow(int? budgetId = null)
        {
            InitializeComponent();
            _budgetId = budgetId;
            BudgetDetailsDataGrid.ItemsSource = _budgetItems;
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            YearComboBox.ItemsSource = Enumerable.Range(DateTime.Now.Year, 10);

            if (_budgetId.HasValue) // وضع التعديل
            {
                using (var db = new DatabaseContext())
                {
                    var budget = db.Budgets.Include(b => b.BudgetDetails).ThenInclude(bd => bd.Account)
                                   .FirstOrDefault(b => b.Id == _budgetId.Value);
                    if (budget != null)
                    {
                        BudgetNameTextBox.Text = budget.Name;
                        YearComboBox.SelectedItem = budget.Year;

                        // تحميل الحسابات الموجودة في الموازنة
                        foreach (var detail in budget.BudgetDetails)
                        {
                            _budgetItems.Add(new BudgetDetailViewModel
                            {
                                AccountId = detail.AccountId,
                                AccountName = detail.Account.AccountName,
                                AccountNumber = detail.Account.AccountNumber,
                                Month1 = detail.Month1Amount,
                                Month2 = detail.Month2Amount,
                                Month3 = detail.Month3Amount,
                                Month4 = detail.Month4Amount,
                                Month5 = detail.Month5Amount,
                                Month6 = detail.Month6Amount,
                                Month7 = detail.Month7Amount,
                                Month8 = detail.Month8Amount,
                                Month9 = detail.Month9Amount,
                                Month10 = detail.Month10Amount,
                                Month11 = detail.Month11Amount,
                                Month12 = detail.Month12Amount,
                            });
                        }
                    }
                }
            }
            else // وضع الإضافة
            {
                YearComboBox.SelectedItem = DateTime.Now.Year;
                // تحميل جميع حسابات الإيرادات والمصروفات
                using (var db = new DatabaseContext())
                {
                    var accountsToBudget = db.Accounts
                        .Where(a => a.AccountType == AccountType.Revenue || a.AccountType == AccountType.Expense)
                        .OrderBy(a => a.AccountNumber)
                        .ToList();

                    foreach (var account in accountsToBudget)
                    {
                        _budgetItems.Add(new BudgetDetailViewModel
                        {
                            AccountId = account.Id,
                            AccountName = account.AccountName,
                            AccountNumber = account.AccountNumber
                        });
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BudgetNameTextBox.Text) || YearComboBox.SelectedItem == null)
            {
                MessageBox.Show("يرجى إدخال اسم الموازنة والسنة.", "بيانات ناقصة");
                return;
            }

            try
            {
                using (var db = new DatabaseContext())
                {
                    Budget budget;
                    if (_budgetId.HasValue)
                    {
                        budget = db.Budgets.Include(b => b.BudgetDetails).FirstOrDefault(b => b.Id == _budgetId.Value);
                        if (budget == null) return;
                    }
                    else
                    {
                        budget = new Budget();
                        db.Budgets.Add(budget);
                    }

                    budget.Name = BudgetNameTextBox.Text;
                    budget.Year = (int)YearComboBox.SelectedItem;

                    // تحديث أو إضافة التفاصيل
                    foreach (var itemVM in _budgetItems)
                    {
                        var detail = budget.BudgetDetails.FirstOrDefault(d => d.AccountId == itemVM.AccountId);
                        if (detail == null)
                        {
                            detail = new BudgetDetail { AccountId = itemVM.AccountId };
                            budget.BudgetDetails.Add(detail);
                        }

                        detail.Month1Amount = itemVM.Month1;
                        detail.Month2Amount = itemVM.Month2;
                        detail.Month3Amount = itemVM.Month3;
                        detail.Month4Amount = itemVM.Month4;
                        detail.Month5Amount = itemVM.Month5;
                        detail.Month6Amount = itemVM.Month6;
                        detail.Month7Amount = itemVM.Month7;
                        detail.Month8Amount = itemVM.Month8;
                        detail.Month9Amount = itemVM.Month9;
                        detail.Month10Amount = itemVM.Month10;
                        detail.Month11Amount = itemVM.Month11;
                        detail.Month12Amount = itemVM.Month12;
                    }

                    db.SaveChanges();
                    MessageBox.Show("تم حفظ الموازنة بنجاح.", "نجاح");
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ الموازنة: {ex.InnerException?.Message ?? ex.Message}", "خطأ");
            }
        }
    }
}
