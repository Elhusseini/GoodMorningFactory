// GoodMorningFactory/UI/ViewModels/SupplierViewModel.cs
namespace GoodMorningFactory.UI.ViewModels
{
    public class SupplierViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string SupplierCode { get; set; }
        public string Name { get; set; }
        public string ContactPerson { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public decimal CurrentBalance { get; set; }
    }
}
