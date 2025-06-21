// UI/Views/AddEditFixedAssetWindow.xaml.cs
// *** تحديث: تم إصلاح آلية تحميل وتعيين الحسابات ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.ComponentModel;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditFixedAssetWindow : Window
    {
        private readonly int? _assetId;

        public AddEditFixedAssetWindow(int? assetId = null)
        {
            InitializeComponent();
            _assetId = assetId;
            LoadComboBoxes();
            if (_assetId.HasValue)
            {
                LoadAssetData();
            }
        }

        private void LoadComboBoxes()
        {
            DepreciationMethodComboBox.ItemsSource = Enum.GetValues(typeof(DepreciationMethod))
                .Cast<DepreciationMethod>()
                .Select(e => new { Value = e, Description = GetEnumDescription(e) });

            using (var db = new DatabaseContext())
            {
                var accounts = db.Accounts.OrderBy(a => a.AccountNumber).ToList();
                AssetAccountComboBox.ItemsSource = accounts.Where(a => a.AccountType == AccountType.Asset).ToList();
                AccumulatedDepreciationComboBox.ItemsSource = accounts.Where(a => a.AccountType == AccountType.Asset).ToList();
                DepreciationExpenseComboBox.ItemsSource = accounts.Where(a => a.AccountType == AccountType.Expense).ToList();
            }
        }

        private static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute == null ? value.ToString() : attribute.Description;
        }

        private void LoadAssetData()
        {
            using (var db = new DatabaseContext())
            {
                var asset = db.FixedAssets.Find(_assetId);
                if (asset != null)
                {
                    AssetNameTextBox.Text = asset.AssetName;
                    AssetCodeTextBox.Text = asset.AssetCode;
                    DescriptionTextBox.Text = asset.Description;
                    AcquisitionDatePicker.SelectedDate = asset.AcquisitionDate;
                    AcquisitionCostTextBox.Text = asset.AcquisitionCost.ToString();
                    SalvageValueTextBox.Text = asset.SalvageValue.ToString();
                    UsefulLifeTextBox.Text = asset.UsefulLifeYears.ToString();
                    DepreciationMethodComboBox.SelectedValue = asset.DepreciationMethod;
                    AssetAccountComboBox.SelectedValue = asset.AssetAccountId;
                    AccumulatedDepreciationComboBox.SelectedValue = asset.AccumulatedDepreciationAccountId;
                    DepreciationExpenseComboBox.SelectedValue = asset.DepreciationExpenseAccountId;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AssetNameTextBox.Text) || AssetAccountComboBox.SelectedItem == null || AccumulatedDepreciationComboBox.SelectedItem == null || DepreciationExpenseComboBox.SelectedItem == null)
            {
                MessageBox.Show("يرجى ملء جميع الحقول المطلوبة، بما في ذلك جميع الحسابات المرتبطة.", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            {
                FixedAsset asset;
                if (_assetId.HasValue)
                {
                    asset = db.FixedAssets.Find(_assetId.Value);
                }
                else
                {
                    asset = new FixedAsset();
                    db.FixedAssets.Add(asset);
                }

                asset.AssetName = AssetNameTextBox.Text;
                asset.AssetCode = AssetCodeTextBox.Text;
                asset.Description = DescriptionTextBox.Text;
                asset.AcquisitionDate = AcquisitionDatePicker.SelectedDate ?? DateTime.Now;
                decimal.TryParse(AcquisitionCostTextBox.Text, out var cost);
                asset.AcquisitionCost = cost;
                decimal.TryParse(SalvageValueTextBox.Text, out var salvage);
                asset.SalvageValue = salvage;
                int.TryParse(UsefulLifeTextBox.Text, out var life);
                asset.UsefulLifeYears = life;
                asset.DepreciationMethod = (DepreciationMethod)DepreciationMethodComboBox.SelectedValue;
                asset.AssetAccountId = (int)AssetAccountComboBox.SelectedValue;
                asset.AccumulatedDepreciationAccountId = (int)AccumulatedDepreciationComboBox.SelectedValue;
                asset.DepreciationExpenseAccountId = (int)DepreciationExpenseComboBox.SelectedValue;

                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}
