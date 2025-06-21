// UI/Views/AddEditCustomerWindow.xaml.cs
// الكود الخلفي لنافذة إضافة وتعديل عميل
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditCustomerWindow : Window
    {
        private Customer _customerToEdit;

        public AddEditCustomerWindow(Customer customer = null)
        {
            InitializeComponent();
            _customerToEdit = customer;

            if (_customerToEdit != null) // وضع التعديل
            {
                Title = $"تعديل بيانات العميل: {_customerToEdit.CustomerName}";
                PopulateFields();
            }
            else // وضع الإضافة
            {
                Title = "إضافة عميل جديد";
                IsActiveCheckBox.IsChecked = true;
                CustomerCodeTextBox.Text = $"CUST-{DateTime.Now:yyyyMMddHHmmss}";
            }
        }

        private void PopulateFields()
        {
            CustomerCodeTextBox.Text = _customerToEdit.CustomerCode;
            CustomerNameTextBox.Text = _customerToEdit.CustomerName;
            ContactPersonTextBox.Text = _customerToEdit.ContactPerson;
            PhoneNumberTextBox.Text = _customerToEdit.PhoneNumber;
            EmailTextBox.Text = _customerToEdit.Email;
            TaxNumberTextBox.Text = _customerToEdit.TaxNumber;
            IsActiveCheckBox.IsChecked = _customerToEdit.IsActive;
            BillingAddressTextBox.Text = _customerToEdit.BillingAddress;
            ShippingAddressTextBox.Text = _customerToEdit.ShippingAddress;
            PaymentTermsTextBox.Text = _customerToEdit.DefaultPaymentTerms;
            CreditLimitTextBox.Text = _customerToEdit.CreditLimit.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text))
            {
                MessageBox.Show("اسم العميل حقل مطلوب.", "بيانات غير مكتملة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DatabaseContext())
            {
                if (_customerToEdit == null) // إضافة
                {
                    _customerToEdit = new Customer();
                    db.Customers.Add(_customerToEdit);
                }
                else // تعديل
                {
                    db.Customers.Attach(_customerToEdit);
                }

                // تحديث البيانات من الحقول
                _customerToEdit.CustomerCode = CustomerCodeTextBox.Text;
                _customerToEdit.CustomerName = CustomerNameTextBox.Text;
                _customerToEdit.ContactPerson = ContactPersonTextBox.Text;
                _customerToEdit.PhoneNumber = PhoneNumberTextBox.Text;
                _customerToEdit.Email = EmailTextBox.Text;
                _customerToEdit.TaxNumber = TaxNumberTextBox.Text;
                _customerToEdit.IsActive = IsActiveCheckBox.IsChecked ?? true;
                _customerToEdit.BillingAddress = BillingAddressTextBox.Text;
                _customerToEdit.ShippingAddress = ShippingAddressTextBox.Text;
                _customerToEdit.DefaultPaymentTerms = PaymentTermsTextBox.Text;
                if (decimal.TryParse(CreditLimitTextBox.Text, out decimal creditLimit))
                {
                    _customerToEdit.CreditLimit = creditLimit;
                }

                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}