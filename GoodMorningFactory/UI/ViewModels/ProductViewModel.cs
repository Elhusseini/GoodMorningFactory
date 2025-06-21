// UI/ViewModels/ProductViewModel.cs
// *** الكود الكامل والصحيح لـ ViewModel الخاص ببيانات المنتجات ***
namespace GoodMorningFactory.UI.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string ProductCode { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public string ProductType { get; set; }
        public decimal SalePrice { get; set; }
        public int CurrentStock { get; set; }
        public bool IsActive { get; set; }
        public string CurrencySymbol { get; set; }
    }
}