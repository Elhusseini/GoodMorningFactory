// UI/Views/AddEditCurrencyWindow.xaml.cs
// *** تحديث: تمت إضافة منطق لحفظ وتحميل حقول التفقيط ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditCurrencyWindow : Window
    {
        private int? _currencyId;
        public AddEditCurrencyWindow(int? currencyId = null)
        {
            InitializeComponent();
            _currencyId = currencyId;
            if (_currencyId.HasValue)
                LoadCurrency();
            else
                IsActiveCheckBox.IsChecked = true;
        }

        private void LoadCurrency()
        {
            using (var db = new DatabaseContext())
            {
                var currency = db.Currencies.Find(_currencyId.Value);
                if (currency != null)
                {
                    NameTextBox.Text = currency.Name;
                    SymbolTextBox.Text = currency.Symbol;
                    CodeTextBox.Text = currency.Code;
                    IsActiveCheckBox.IsChecked = currency.IsActive;
                    // --- بداية الإضافة ---
                    CurrencyNameArTextBox.Text = currency.CurrencyName_AR;
                    FractionalUnitArTextBox.Text = currency.FractionalUnit_AR;
                    // --- نهاية الإضافة ---
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || string.IsNullOrWhiteSpace(SymbolTextBox.Text) || string.IsNullOrWhiteSpace(CodeTextBox.Text))
            {
                MessageBox.Show("يرجى ملء الحقول الأساسية (الاسم، الرمز، الكود).", "بيانات ناقصة");
                return;
            }
            using (var db = new DatabaseContext())
            {
                Currency currency;
                if (_currencyId.HasValue)
                {
                    currency = db.Currencies.Find(_currencyId.Value);
                    if (currency == null) return;
                }
                else
                {
                    currency = new Currency();
                    db.Currencies.Add(currency);
                }
                currency.Name = NameTextBox.Text.Trim();
                currency.Symbol = SymbolTextBox.Text.Trim();
                currency.Code = CodeTextBox.Text.Trim().ToUpper();
                currency.IsActive = IsActiveCheckBox.IsChecked ?? true;
                // --- بداية الإضافة ---
                currency.CurrencyName_AR = CurrencyNameArTextBox.Text.Trim();
                currency.FractionalUnit_AR = FractionalUnitArTextBox.Text.Trim();
                // --- نهاية الإضافة ---
                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}
