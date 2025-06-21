// GoodMorningFactory/UI/ViewModels/RunDepreciationViewModel.cs
// *** الكود الكامل والمصحح ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class RunDepreciationViewModel : BaseViewModel
    {
        private readonly IFixedAssetService _assetService;

        private JournalVoucher _proposedVoucher;
        public JournalVoucher ProposedVoucher
        {
            get => _proposedVoucher;
            set
            {
                _proposedVoucher = value;
                OnPropertyChanged();
                // ======================= بداية الإصلاح =======================
                // عند تغيير القيد المقترح، نُعلم الواجهة بتحديث خاصية الإظهار
                OnPropertyChanged(nameof(HasProposedVoucher));
                // ======================== نهاية الإصلاح ========================
            }
        }

        // ======================= بداية الإصلاح =======================
        /// <summary>
        /// خاصية جديدة تتحكم في إظهار قسم النتائج في الواجهة.
        /// </summary>
        public bool HasProposedVoucher => ProposedVoucher != null;
        // ======================== نهاية الإصلاح ========================

        private Dictionary<int, decimal> _depreciatedAssets;

        public List<int> Months { get; } = Enumerable.Range(1, 12).ToList();
        public List<int> Years { get; } = Enumerable.Range(DateTime.Now.Year - 5, 10).Reverse().ToList();

        private int _selectedMonth;
        public int SelectedMonth
        {
            get => _selectedMonth;
            set { _selectedMonth = value; OnPropertyChanged(); ResetView(); }
        }

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set { _selectedYear = value; OnPropertyChanged(); ResetView(); }
        }

        private bool _isCalculating;
        public bool IsCalculating
        {
            get => _isCalculating;
            set { _isCalculating = value; OnPropertyChanged(); }
        }

        public ICommand CalculateCommand { get; }
        public ICommand PostVoucherCommand { get; }

        public RunDepreciationViewModel(IFixedAssetService service)
        {
            _assetService = service;
            SelectedMonth = DateTime.Now.Month;
            SelectedYear = DateTime.Now.Year;

            // *** بداية الإصلاح: تعديل تعريف الأوامر ***
            CalculateCommand = new RelayCommand(async (param) => await CalculateDepreciationAsync(), (param) => !IsCalculating);
            // ======================= بداية الإصلاح =======================
            // تم تحديث شرط التنفيذ ليعتمد على الخاصية الجديدة
            PostVoucherCommand = new RelayCommand(async (p) => await PostVoucherAsync(p as Window), (p) => HasProposedVoucher && !IsCalculating);
            // ======================== نهاية الإصلاح ========================
        }

        private async Task CalculateDepreciationAsync()
        {
            IsCalculating = true;
            try
            {
                var periodEndDate = new DateTime(SelectedYear, SelectedMonth, DateTime.DaysInMonth(SelectedYear, SelectedMonth));
                // سنفترض أن الخدمة ستعيد القيد وتفاصيل الأصول معاً
                var (voucher, details) = await _assetService.CalculateDepreciationVoucherAsync(periodEndDate);
                ProposedVoucher = voucher;
                _depreciatedAssets = details;

                if (ProposedVoucher == null)
                {
                    MessageBox.Show("لا توجد أصول قابلة للإهلاك أو أن قيمة الإهلاك صفر لهذه الفترة.", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "معلومة", MessageBoxButton.OK, MessageBoxImage.Warning);
                ResetView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل احتساب الإهلاك: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetView();
            }
            finally
            {
                IsCalculating = false;
            }
        }

        private async Task PostVoucherAsync(Window window)
        {
            if (!HasProposedVoucher) return;

            var confirmation = MessageBox.Show($"سيتم ترحيل قيد إهلاك بقيمة إجمالية {ProposedVoucher.TotalDebit:N2}. هل أنت متأكد؟", "تأكيد الترحيل", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmation != MessageBoxResult.Yes) return;

            IsCalculating = true;
            try
            {
                await _assetService.PostDepreciationVoucherAsync(ProposedVoucher, _depreciatedAssets);
                MessageBox.Show("تم ترحيل قيد الإهلاك بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                window.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل ترحيل القيد: {ex.Message}", "خطأ");
            }
            finally
            {
                IsCalculating = false;
                ResetView();
            }
        }

        private void ResetView()
        {
            ProposedVoucher = null;
        }
    }
}