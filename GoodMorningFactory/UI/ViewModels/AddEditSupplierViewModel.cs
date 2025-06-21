// GoodMorningFactory/UI/ViewModels/AddEditSupplierViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel لنافذة إضافة وتعديل الموردين.
    /// </summary>
    public class AddEditSupplierViewModel : BaseViewModel
    {
        private readonly ISupplierService _supplierService;
        private Supplier _supplier;

        #region Properties
        public string WindowTitle { get; private set; }
        public string SupplierCode { get => _supplier.SupplierCode; set { _supplier.SupplierCode = value; OnPropertyChanged(); } }
        [Required(ErrorMessage = "اسم المورد مطلوب")]
        public string Name { get => _supplier.Name; set { _supplier.Name = value; OnPropertyChanged(); } }
        public string ContactPerson { get => _supplier.ContactPerson; set { _supplier.ContactPerson = value; OnPropertyChanged(); } }
        public string PhoneNumber { get => _supplier.PhoneNumber; set { _supplier.PhoneNumber = value; OnPropertyChanged(); } }
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string Email { get => _supplier.Email; set { _supplier.Email = value; OnPropertyChanged(); } }
        public string Website { get => _supplier.Website; set { _supplier.Website = value; OnPropertyChanged(); } }
        public bool IsActive { get => _supplier.IsActive; set { _supplier.IsActive = value; OnPropertyChanged(); } }
        public string Address { get => _supplier.Address; set { _supplier.Address = value; OnPropertyChanged(); } }
        public string DefaultPaymentTerms { get => _supplier.DefaultPaymentTerms; set { _supplier.DefaultPaymentTerms = value; OnPropertyChanged(); } }
        #endregion

        public AddEditSupplierViewModel(ISupplierService supplierService, int? supplierId)
        {
            _supplierService = supplierService;
            LoadSupplierAsync(supplierId);
        }

        private async void LoadSupplierAsync(int? supplierId)
        {
            if (supplierId.HasValue)
            {
                _supplier = await _supplierService.GetSupplierByIdAsync(supplierId.Value);
                WindowTitle = $"تعديل بيانات المورد: {_supplier.Name}";
            }
            else
            {
                _supplier = new Supplier
                {
                    IsActive = true,
                    SupplierCode = await _supplierService.GetNextSupplierCodeAsync()
                };
                WindowTitle = "إضافة مورد جديد";
            }
            // تحديث كل الخصائص في الواجهة
            OnPropertyChanged(string.Empty);
        }

        public async Task<bool> SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("اسم المورد حقل مطلوب.", "بيانات غير مكتملة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            try
            {
                if (_supplier.Id > 0) // تحديث
                {
                    await _supplierService.UpdateSupplierAsync(_supplier);
                }
                else // إضافة
                {
                    await _supplierService.AddSupplierAsync(_supplier);
                }
                return true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
