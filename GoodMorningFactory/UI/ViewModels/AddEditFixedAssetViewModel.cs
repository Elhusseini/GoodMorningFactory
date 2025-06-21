// GoodMorningFactory/UI/ViewModels/AddEditFixedAssetViewModel.cs
// *** الكود الكامل والنهائي بعد إصلاح خطأ الربط مع الحسابات ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditFixedAssetViewModel : BaseViewModel
    {
        private readonly IFixedAssetService _assetService;
        private FixedAsset _asset;
        public FixedAsset Asset
        {
            get => _asset;
            set { _asset = value; OnPropertyChanged(); }
        }

        public string WindowTitle { get; private set; }
        public ObservableCollection<Account> AssetAccounts { get; set; } = new ObservableCollection<Account>();
        public ObservableCollection<Account> ExpenseAccounts { get; set; } = new ObservableCollection<Account>();
        public IEnumerable<DepreciationMethodViewModel> DepreciationMethods { get; }

        public RelayCommand SaveCommand { get; }

        public AddEditFixedAssetViewModel(IFixedAssetService service, int? assetId)
        {
            _assetService = service;
            SaveCommand = new RelayCommand(async (p) => await SaveAsync(p as Window), CanSave);

            DepreciationMethods = Enum.GetValues(typeof(DepreciationMethod))
                .Cast<DepreciationMethod>()
                .Select(e => new DepreciationMethodViewModel { Method = e, Description = GetEnumDescription(e) });

            LoadDataAsync(assetId);
        }

        private async void LoadDataAsync(int? assetId)
        {
            try
            {
                var assetAccountsList = await _assetService.GetAssetAccountsAsync();
                var expenseAccountsList = await _assetService.GetExpenseAccountsAsync();

                AssetAccounts.Clear();
                foreach (var acc in assetAccountsList) AssetAccounts.Add(acc);

                ExpenseAccounts.Clear();
                foreach (var acc in expenseAccountsList) ExpenseAccounts.Add(acc);

                if (assetId.HasValue && assetId != 0)
                {
                    WindowTitle = "تعديل أصل ثابت";
                    Asset = await _assetService.GetAssetByIdAsync(assetId.Value);
                }
                else
                {
                    WindowTitle = "إضافة أصل ثابت جديد";
                    Asset = new FixedAsset
                    {
                        AcquisitionDate = DateTime.Now,
                        DepreciationMethod = GoodMorningFactory.Data.Models.DepreciationMethod.StraightLine
                    };
                }
                OnPropertyChanged(nameof(WindowTitle));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        private bool CanSave(object parameter)
        {
            return Asset != null &&
                   !string.IsNullOrWhiteSpace(Asset.AssetName) &&
                   Asset.AcquisitionCost > 0 &&
                   Asset.UsefulLifeYears > 0 &&
                   Asset.AssetAccountId != 0 &&
                   Asset.AccumulatedDepreciationAccountId != 0 &&
                   Asset.DepreciationExpenseAccountId != 0;
        }

        private async Task SaveAsync(Window window)
        {
            try
            {
                // ======================= بداية الإصلاح الرئيسي =======================
                // قبل إرسال كائن الأصل للحفظ، نقوم بتفريغ خصائص الربط المباشر.
                // هذا يخبر Entity Framework بالاعتماد فقط على الـ IDs (مثل AssetAccountId)
                // للربط مع الحسابات الموجودة، بدلاً من محاولة إنشاء حسابات جديدة.
                Asset.AssetAccount = null;
                Asset.AccumulatedDepreciationAccount = null;
                Asset.DepreciationExpenseAccount = null;
                // ======================== نهاية الإصلاح الرئيسي ========================

                await _assetService.SaveAssetAsync(Asset);
                window.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ الأصل. تأكد من أن جميع الحقول المطلوبة مملوءة.\n\nالتفاصيل: {ex.InnerException?.Message ?? ex.Message}", "خطأ");
            }
        }

        private static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }

    public class DepreciationMethodViewModel
    {
        public DepreciationMethod Method { get; set; }
        public string Description { get; set; }
    }
}