// UI/Views/BudgetsView.xaml.cs
// *** ملف جديد: الكود الخلفي لواجهة إدارة الموازنات ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class BudgetsView : UserControl
    {
        public BudgetsView()
        {
            InitializeComponent();
            LoadBudgets();
        }

        private void LoadBudgets()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    BudgetsDataGrid.ItemsSource = db.Budgets.OrderByDescending(b => b.Year).ThenBy(b => b.Name).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل الموازنات: {ex.Message}", "خطأ");
            }
        }

        private void AddBudgetButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditBudgetWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadBudgets();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Budget budget)
            {
                var editWindow = new AddEditBudgetWindow(budget.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadBudgets();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Budget budget)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف الموازنة '{budget.Name}'؟ سيتم حذف جميع تفاصيلها.", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DatabaseContext())
                        {
                            var budgetToDelete = db.Budgets.Find(budget.Id);
                            if (budgetToDelete != null)
                            {
                                db.Budgets.Remove(budgetToDelete); // الحذف المتتالي سيهتم بالتفاصيل
                                db.SaveChanges();
                                LoadBudgets();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"فشل حذف الموازنة: {ex.Message}", "خطأ");
                    }
                }
            }
        }
    }
}
