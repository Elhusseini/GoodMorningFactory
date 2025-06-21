// GoodMorningFactory/UI/ViewModels/SalesQuotationsViewModel.cs
using GoodMorningFactory.Core.Helpers;
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class SalesQuotationsViewModel : BaseViewModel
    {
        private readonly ISalesQuotationService _quotationService;
        private readonly IFilterService _filterService;
        private readonly IPrintingService _printingService;

        #region Properties
        private ObservableCollection<SalesQuotation> _quotations;
        public ObservableCollection<SalesQuotation> Quotations { get => _quotations; set { _quotations = value; OnPropertyChanged(); } }

        private string _searchText = "";
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ResetAndLoad(); } }

        public List<object> StatusFilters { get; private set; }
        private object _selectedStatusFilter;
        public object SelectedStatusFilter { get => _selectedStatusFilter; set { _selectedStatusFilter = value; OnPropertyChanged(); ResetAndLoad(); } }

        private DateTime? _fromDate;
        public DateTime? FromDate { get => _fromDate; set { _fromDate = value; OnPropertyChanged(); ResetAndLoad(); } }

        private DateTime? _toDate;
        public DateTime? ToDate { get => _toDate; set { _toDate = value; OnPropertyChanged(); ResetAndLoad(); } }

        private string _pageInfo;
        public string PageInfo { get => _pageInfo; set { _pageInfo = value; OnPropertyChanged(); } }

        private int _currentPage = 1;
        private readonly int _pageSize = 15;
        private int _totalItems = 0;
        #endregion

        #region Commands
        public ICommand AddQuotationCommand { get; }
        public ICommand EditQuotationCommand { get; }
        public ICommand DeleteQuotationCommand { get; }
        public ICommand ConvertToOrderCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        #endregion

        public SalesQuotationsViewModel()
        {
            _quotationService = new SalesQuotationService();
            _filterService = new FilterService();
            _printingService = new PrintingService();

            AddQuotationCommand = new RelayCommand(AddQuotation);
            EditQuotationCommand = new RelayCommand(EditQuotation, CanActOnQuotation);
            DeleteQuotationCommand = new RelayCommand(DeleteQuotation, CanActOnQuotation);
            ConvertToOrderCommand = new RelayCommand(ConvertToOrder, CanConvertToOrder); // <-- تم تعديل شرط التنفيذ
            PrintCommand = new RelayCommand(ExecutePrintQuotation, CanActOnQuotation);
            NextPageCommand = new RelayCommand(GoToNextPage, _ => _currentPage < GetTotalPages());
            PreviousPageCommand = new RelayCommand(GoToPreviousPage, _ => _currentPage > 1);

            Initialize();
        }

        private async void Initialize()
        {
            LoadFilters();
            await LoadQuotationsAsync();
        }

        private void LoadFilters()
        {
            var statuses = new List<object> { "الكل" };
            statuses.AddRange(Enum.GetValues(typeof(QuotationStatus)).Cast<object>());
            StatusFilters = statuses;
            SelectedStatusFilter = StatusFilters.First();
        }

        private async Task LoadQuotationsAsync()
        {
            try
            {
                var criteria = new QuotationFilterCriteria
                {
                    Page = _currentPage,
                    PageSize = _pageSize,
                    SearchText = SearchText,
                    Status = (SelectedStatusFilter is QuotationStatus status) ? status : (QuotationStatus?)null,
                    FromDate = FromDate,
                    ToDate = ToDate
                };
                var result = await _quotationService.GetQuotationsAsync(criteria);
                Quotations = new ObservableCollection<SalesQuotation>(result.Items);
                _totalItems = result.TotalCount;
                UpdatePageInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل عروض الأسعار: {ex.Message}", "خطأ");
            }
        }

        private void AddQuotation(object parameter)
        {
            var addWindow = new AddEditSalesQuotationWindow();
            if (addWindow.ShowDialog() == true) ResetAndLoad();
        }

        private void EditQuotation(object parameter)
        {
            if (parameter is SalesQuotation quotation)
            {
                var editWindow = new AddEditSalesQuotationWindow(quotation.Id);
                if (editWindow.ShowDialog() == true) LoadQuotationsAsync();
            }
        }

        private async void DeleteQuotation(object parameter)
        {
            if (parameter is SalesQuotation quotation)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف عرض السعر '{quotation.QuotationNumber}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                try
                {
                    await _quotationService.DeleteQuotationAsync(quotation.Id);
                    await LoadQuotationsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل الحذف: {ex.Message}", "خطأ");
                }
            }
        }

        // --- بداية التعديل: تحسين منطق دالة التحويل وشرط التنفيذ ---

        /// <summary>
        /// شرط لتحديد ما إذا كان يمكن تحويل عرض السعر إلى أمر بيع.
        /// التحويل مسموح فقط إذا كان عرض السعر في حالة "مقبول".
        /// </summary>
        private bool CanConvertToOrder(object parameter)
        {
            return parameter is SalesQuotation quotation && quotation.Status == QuotationStatus.Accepted;
        }

        /// <summary>
        /// الدالة التي تنفذ عملية التحويل.
        /// </summary>
        private async void ConvertToOrder(object parameter)
        {
            if (parameter is SalesQuotation quotation)
            {
                // الخطوة 1: تأكيد من المستخدم قبل المتابعة
                var result = MessageBox.Show($"سيتم إنشاء أمر بيع جديد من عرض السعر رقم '{quotation.QuotationNumber}'.\nهل تريد المتابعة؟", "تأكيد التحويل", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                // الخطوة 2: فتح نافذة إنشاء أمر البيع، مع تمرير رقم عرض السعر كمصدر للبيانات
                var orderWindow = new AddEditSalesOrderWindow(sourceQuotationId: quotation.Id);

                // الخطوة 3: التحقق من نتيجة إغلاق النافذة. لن يتم تنفيذ هذا الكود إلا إذا ضغط المستخدم على "حفظ"
                if (orderWindow.ShowDialog() == true)
                {
                    try
                    {
                        // الخطوة 4: الآن فقط، بعد الحفظ الناجح لأمر البيع، نقوم بتحديث حالة عرض السعر إلى "مغلق"
                        await _quotationService.UpdateQuotationStatusAsync(quotation.Id, QuotationStatus.Closed);
                        MessageBox.Show("تم إنشاء أمر البيع وتحديث حالة عرض السعر بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                        // الخطوة 5: إعادة تحميل القائمة لإظهار الحالة المحدثة لعرض السعر
                        await LoadQuotationsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"حدث خطأ أثناء تحديث حالة عرض السعر: {ex.Message}", "خطأ");
                    }
                }
                // ملاحظة: إذا أغلق المستخدم نافذة أمر البيع (إلغاء)، لن يتم تنفيذ أي شيء وستبقى حالة عرض السعر "مقبول"
            }
        }
        // --- نهاية التعديل ---

        private async void ExecutePrintQuotation(object parameter)
        {
            if (parameter is SalesQuotation quotation)
            {
                await _printingService.PrintSalesQuotationAsync(quotation.Id);
            }
        }

        private bool CanActOnQuotation(object parameter) => parameter is SalesQuotation;

        #region Pagination Helpers
        private async void ResetAndLoad() { _currentPage = 1; await LoadQuotationsAsync(); }
        private async void GoToNextPage(object p) { if (_currentPage < GetTotalPages()) { _currentPage++; await LoadQuotationsAsync(); } }
        private async void GoToPreviousPage(object p) { if (_currentPage > 1) { _currentPage--; await LoadQuotationsAsync(); } }
        private int GetTotalPages() => (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / _pageSize);
        private void UpdatePageInfo() => PageInfo = $"الصفحة {_currentPage} من {GetTotalPages()} (إجمالي العروض: {_totalItems})";
        #endregion
    }
}