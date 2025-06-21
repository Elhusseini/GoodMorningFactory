// GoodMorningFactory/UI/Views/AddEditSalesQuotationWindow.xaml.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using System.ComponentModel;
using System.Windows;
using GoodMorningFactory.Core.Services; // قد تحتاج هذا

namespace GoodMorningFactory.UI.Views
{
    // تم نقل هذا الكلاس المساعد إلى هنا ليبقى ضمن نطاق النافذة
    public class SalesQuotationItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }

        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set { if (_unitPrice != value) { _unitPrice = value; OnAllPropertiesChanged(); } }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set { if (_quantity != value) { _quantity = value; OnAllPropertiesChanged(); } }
        }

        private decimal _discount;
        public decimal Discount
        {
            get => _discount;
            set { if (_discount != value) { _discount = value; OnAllPropertiesChanged(); } }
        }

        public decimal Subtotal => (UnitPrice * Quantity) - Discount;
        public string SubtotalFormatted => $"{Subtotal:N2} {AppSettings.DefaultCurrencySymbol}";

        public event PropertyChangedEventHandler PropertyChanged;

        // دالة لتحديث كل الخصائص المحسوبة مرة واحدة
        private void OnAllPropertiesChanged()
        {
            OnPropertyChanged(nameof(UnitPrice));
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(Discount));
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(SubtotalFormatted));
        }
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// أصبح الكود الخلفي الآن مسؤولاً فقط عن إنشاء النافذة وتمرير البيانات إلى الـ ViewModel.
    /// </summary>
    public partial class AddEditSalesQuotationWindow : Window
    {
        // بنّاء للتعديل
        public AddEditSalesQuotationWindow(int? quotationId = null)
        {
            InitializeComponent();
            DataContext = new AddEditSalesQuotationViewModel(quotationId, null);
        }

        // بنّاء جديد للإنشاء من فرصة بيعية
        public AddEditSalesQuotationWindow(Opportunity sourceOpportunity)
        {
            InitializeComponent();
            DataContext = new AddEditSalesQuotationViewModel(null, sourceOpportunity);
        }
    }
}