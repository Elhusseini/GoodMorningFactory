// UI/Views/AddEditBillOfMaterialsWindow.xaml.cs
// أصبح الكود الخلفي نظيفاً جداً الآن
using GoodMorningFactory.UI.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    // لا يزال هذا الكلاس مطلوباً لمشاركة البيانات بين ال ViewModel والواجهة
    public class BillOfMaterialsItemViewModel : INotifyPropertyChanged
    {
        public int RawMaterialId { get; set; }
        public string RawMaterialName { get; set; }

        private decimal _quantity;
        public decimal Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(nameof(Quantity)); }
        }

        private decimal _scrapPercentage;
        public decimal ScrapPercentage
        {
            get => _scrapPercentage;
            set { _scrapPercentage = value; OnPropertyChanged(nameof(ScrapPercentage)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public partial class AddEditBillOfMaterialsWindow : Window
    {
        // تم نقل كل المنطق البرمجي إلى الـ ViewModel
        public AddEditBillOfMaterialsWindow(int? bomId = null, int? sourceBomIdToCopy = null)
        {
            InitializeComponent();
            // إنشاء الـ ViewModel وربطه بالـ DataContext للنافذة
            DataContext = new AddEditBillOfMaterialsViewModel(bomId, sourceBomIdToCopy);
        }
    }
}