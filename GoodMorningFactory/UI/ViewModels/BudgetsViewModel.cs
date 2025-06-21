// GoodMorningFactory/UI/ViewModels/BudgetsViewModel.cs
// *** ملف جديد: ViewModel لواجهة عرض الموازنات ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class BudgetsViewModel : BaseViewModel
    {
        private readonly IBudgetService _budgetService;
        public ObservableCollection<Budget> Budgets { get; } = new ObservableCollection<Budget>();

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public BudgetsViewModel()
        {
            _budgetService = new BudgetService();
            AddCommand = new RelayCommand(AddBudget);
            EditCommand = new RelayCommand(EditBudget, CanExecute);
            DeleteCommand = new RelayCommand(async (p) => await DeleteBudgetAsync(p), CanExecute);

            LoadDataAsync();
        }

        private bool CanExecute(object parameter) => parameter != null;

        private async void LoadDataAsync()
        {
            try
            {
                var budgetsList = await _budgetService.GetBudgetsAsync();
                Budgets.Clear();
                foreach (var budget in budgetsList)
                {
                    Budgets.Add(budget);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        private void AddBudget(object parameter)
        {
            var addWindow = new AddEditBudgetWindow();
            if (addWindow.ShowDialog() == true) LoadDataAsync();
        }

        private void EditBudget(object parameter)
        {
            if (parameter is Budget budget)
            {
                var editWindow = new AddEditBudgetWindow(budget.Id);
                if (editWindow.ShowDialog() == true) LoadDataAsync();
            }
        }

        private async Task DeleteBudgetAsync(object parameter)
        {
            if (parameter is Budget budget)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف الموازنة '{budget.Name}'؟ سيتم حذف جميع تفاصيلها بشكل دائم.", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _budgetService.DeleteBudgetAsync(budget.Id);
                        Budgets.Remove(budget);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"حدث خطأ أثناء الحذف: {ex.Message}", "خطأ");
                    }
                }
            }
        }
    }
}