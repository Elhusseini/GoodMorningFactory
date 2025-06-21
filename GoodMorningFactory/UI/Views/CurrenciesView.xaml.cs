// UI/Views/CurrenciesView.xaml.cs
// *** تحديث: تم تحويل الكلاس إلى UserControl ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class CurrenciesView : UserControl
    {
        public CurrenciesView()
        {
            InitializeComponent();
            LoadCurrencies();
        }

        private void LoadCurrencies()
        {
            using (var db = new DatabaseContext())
            {
                CurrenciesDataGrid.ItemsSource = db.Currencies.ToList();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddEditCurrencyWindow();
            if (win.ShowDialog() == true)
                LoadCurrencies();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrenciesDataGrid.SelectedItem is Currency currency)
            {
                var win = new AddEditCurrencyWindow(currency.Id);
                if (win.ShowDialog() == true)
                    LoadCurrencies();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrenciesDataGrid.SelectedItem is Currency currency)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف العملة {currency.Name}?", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var db = new DatabaseContext())
                    {
                        var cur = db.Currencies.Find(currency.Id);
                        if (cur != null)
                        {
                            db.Currencies.Remove(cur);
                            db.SaveChanges();
                        }
                    }
                    LoadCurrencies();
                }
            }
        }
    }
}
