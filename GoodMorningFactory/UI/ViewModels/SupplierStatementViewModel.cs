// GoodMorningFactory/UI/ViewModels/SupplierStatementViewModel.cs
using GoodMorningFactory.Core.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel لنافذة عرض كشف حساب المورد.
    /// </summary>
    public class SupplierStatementViewModel : BaseViewModel
    {
        private readonly ISupplierService _supplierService;
        private readonly int _supplierId;

        private string _supplierName;
        public string SupplierName
        {
            get => _supplierName;
            set { _supplierName = value; OnPropertyChanged(); }
        }

        private ObservableCollection<SupplierStatementItemViewModel> _statementItems;
        public ObservableCollection<SupplierStatementItemViewModel> StatementItems
        {
            get => _statementItems;
            set { _statementItems = value; OnPropertyChanged(); }
        }

        public SupplierStatementViewModel(ISupplierService supplierService, int supplierId)
        {
            _supplierService = supplierService;
            _supplierId = supplierId;
        }

        public async Task LoadStatementAsync()
        {
            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(_supplierId);
                if (supplier == null) return;

                SupplierName = $"كشف حساب المورد: {supplier.Name}";
                var items = await _supplierService.GetSupplierStatementAsync(_supplierId);
                StatementItems = new ObservableCollection<SupplierStatementItemViewModel>(items);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل كشف الحساب: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
