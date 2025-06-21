// GoodMorningFactory/UI/ViewModels/AddEditBudgetViewModel.cs
// *** The Complete, Unchanged Code ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditBudgetViewModel : BaseViewModel
    {
        private readonly IBudgetService _budgetService;
        private Budget _budget;
        public Budget Budget
        {
            get => _budget;
            set { _budget = value; OnPropertyChanged(); }
        }

        public string WindowTitle { get; private set; }
        public ObservableCollection<BudgetDetail> BudgetDetails { get; } = new ObservableCollection<BudgetDetail>();
        public ObservableCollection<int> AvailableYears { get; } = new ObservableCollection<int>(Enumerable.Range(DateTime.Now.Year, 10));

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public RelayCommand SaveCommand { get; }

        public AddEditBudgetViewModel(IBudgetService service, int? budgetId)
        {
            _budgetService = service;
            SaveCommand = new RelayCommand(async (p) => await SaveAsync(p as Window), (p) => !IsLoading);
            LoadDataAsync(budgetId);
        }

        private async void LoadDataAsync(int? budgetId)
        {
            IsLoading = true;
            try
            {
                if (budgetId.HasValue) // Edit Mode
                {
                    WindowTitle = "تعديل موازنة تقديرية";
                    Budget = await _budgetService.GetBudgetWithDetailsAsync(budgetId.Value);

                    if (Budget != null && Budget.Description == null)
                    {
                        Budget.Description = string.Empty;
                    }

                    if (Budget != null)
                    {
                        foreach (var detail in Budget.BudgetDetails)
                        {
                            BudgetDetails.Add(detail);
                        }
                    }
                }
                else // Add Mode
                {
                    WindowTitle = "إضافة موازنة تقديرية جديدة";
                    Budget = new Budget { Year = DateTime.Now.Year };

                    var accounts = await _budgetService.GetBudgetableAccountsAsync();
                    foreach (var account in accounts)
                    {
                        BudgetDetails.Add(new BudgetDetail { AccountId = account.Id, Account = account });
                    }
                }
                OnPropertyChanged(nameof(WindowTitle));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load data: {ex.Message}", "Error");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveAsync(Window window)
        {
            if (string.IsNullOrWhiteSpace(Budget.Name) || Budget.Year == 0)
            {
                MessageBox.Show("Budget name and year are required.", "Missing Data");
                return;
            }

            IsLoading = true;
            try
            {
                // We no longer need to null out the Account property here
                // as the service now handles it correctly.

                Budget.BudgetDetails = new Collection<BudgetDetail>(BudgetDetails.ToList());

                await _budgetService.SaveBudgetAsync(Budget);
                MessageBox.Show("Budget saved successfully.", "Success");
                window.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save the budget: {ex.InnerException?.Message ?? ex.Message}", "Error");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}