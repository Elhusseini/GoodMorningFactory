// UI/Views/AddEditSupplierWindow.xaml.cs
// *** الكود الكامل لنافذة إضافة وتعديل مورد مع جميع الحقول الجديدة ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditSupplierWindow : Window
    {
        private Supplier _supplierToEdit;

        public AddEditSupplierWindow(Supplier supplier = null)
        {
            InitializeComponent();
            _supplierToEdit = supplier;

            if (_supplierToEdit != null) // وضع التعديل
            {
                Title = $"تعديل بيانات المورد: {_supplierToEdit.Name}";
                PopulateFields();
            }
            else // وضع الإضافة
            {
                Title = "إضافة مورد جديد";
                IsActiveCheckBox.IsChecked = true;
                SupplierCodeTextBox.Text = $"SUPP-{DateTime.Now:yyyyMMddHHmmss}";
            }
        }

        private void PopulateFields()
        {
            SupplierCodeTextBox.Text = _supplierToEdit.SupplierCode;
            NameTextBox.Text = _supplierToEdit.Name;
            ContactPersonTextBox.Text = _supplierToEdit.ContactPerson;
            PhoneNumberTextBox.Text = _supplierToEdit.PhoneNumber;
            EmailTextBox.Text = _supplierToEdit.Email;
            WebsiteTextBox.Text = _supplierToEdit.Website;
            IsActiveCheckBox.IsChecked = _supplierToEdit.IsActive;
            AddressTextBox.Text = _supplierToEdit.Address;
            PaymentTermsTextBox.Text = _supplierToEdit.DefaultPaymentTerms;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("اسم المورد حقل مطلوب.", "بيانات غير مكتملة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DatabaseContext())
            {
                if (_supplierToEdit == null)
                {
                    _supplierToEdit = new Supplier();
                    db.Suppliers.Add(_supplierToEdit);
                }
                else
                {
                    db.Suppliers.Attach(_supplierToEdit);
                }

                // تحديث البيانات من الحقول
                _supplierToEdit.SupplierCode = SupplierCodeTextBox.Text;
                _supplierToEdit.Name = NameTextBox.Text;
                _supplierToEdit.ContactPerson = ContactPersonTextBox.Text;
                _supplierToEdit.PhoneNumber = PhoneNumberTextBox.Text;
                _supplierToEdit.Email = EmailTextBox.Text;
                _supplierToEdit.Website = WebsiteTextBox.Text;
                _supplierToEdit.IsActive = IsActiveCheckBox.IsChecked ?? true;
                _supplierToEdit.Address = AddressTextBox.Text;
                _supplierToEdit.DefaultPaymentTerms = PaymentTermsTextBox.Text;

                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}