// GoodMorningFactory/UI/ViewModels/SalesReturnsViewModel.cs

// --- ملاحظة: هذا الـ ViewModel مسؤول عن واجهة عرض قائمة مرتجعات المبيعات. ---
// --- يقوم بجلب البيانات من الخدمة وعرضها، ويتعامل مع أوامر المستخدم مثل البحث والطباعة والتنقل بين الصفحات. ---
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.UI.ViewModels
{
    public class SalesReturnsViewModel : BaseViewModel
    {
        private readonly ISalesReturnService _returnService;
        private int _currentPage = 1;
        private const int PageSize = 15;
        private int _totalItems = 0;

        private ObservableCollection<SalesReturn> _returns;
        public ObservableCollection<SalesReturn> Returns { get => _returns; set { _returns = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }
        private DateTime? _fromDate;
        public DateTime? FromDate { get => _fromDate; set { _fromDate = value; OnPropertyChanged(); } }
        private DateTime? _toDate;
        public DateTime? ToDate { get => _toDate; set { _toDate = value; OnPropertyChanged(); } }

        private string _pageInfo;
        public string PageInfo { get => _pageInfo; set { _pageInfo = value; OnPropertyChanged(); } }

        public RelayCommand SearchCommand { get; }
        public RelayCommand PrintCommand { get; }
        public RelayCommand NextPageCommand { get; }
        public RelayCommand PreviousPageCommand { get; }

        public SalesReturnsViewModel()
        {
            _returnService = new SalesReturnService();
            SearchCommand = new RelayCommand(_ => ResetAndLoad());
            PrintCommand = new RelayCommand(PrintCreditNote);
            NextPageCommand = new RelayCommand(async _ => await NextPage(), _ => CanGoNext());
            PreviousPageCommand = new RelayCommand(async _ => await PreviousPage(), _ => CanGoPrevious());

            LoadReturns();
        }

        private async Task LoadReturns()
        {
            var result = await _returnService.GetPagedSalesReturnsAsync(_currentPage, PageSize, SearchText, FromDate, ToDate);
            Returns = new ObservableCollection<SalesReturn>(result.Items);
            _totalItems = result.TotalItems;
            UpdatePageInfo();
        }

        private void ResetAndLoad()
        {
            _currentPage = 1;
            LoadReturns();
        }

        private async void PrintCreditNote(object parameter)
        {
            if (parameter is SalesReturn salesReturn)
            {
                await _returnService.PrintCreditNoteAsync(salesReturn.Id);
            }
        }

        private void UpdatePageInfo()
        {
            int totalPages = (_totalItems == 0) ? 1 : (int)Math.Ceiling((double)_totalItems / PageSize);
            PageInfo = $"الصفحة {_currentPage} من {totalPages} (إجمالي السجلات: {_totalItems})";
            NextPageCommand.RaiseCanExecuteChanged();
            PreviousPageCommand.RaiseCanExecuteChanged();
        }

        private async Task NextPage() { _currentPage++; await LoadReturns(); }
        private bool CanGoNext() => _currentPage < (int)Math.Ceiling((double)_totalItems / PageSize);

        private async Task PreviousPage() { _currentPage--; await LoadReturns(); }
        private bool CanGoPrevious() => _currentPage > 1;
    }
}