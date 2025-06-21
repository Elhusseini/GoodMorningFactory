// UI/Views/NewPurchaseWindow.xaml.cs
// الكود الخلفي لنافذة فاتورة الشراء الجديدة
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    // نستخدم نفس ViewModel الخاص بالبيع مع تعديل بسيط
    public class PurchaseItemViewModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        private decimal _unitPrice;
        private int _quantity;

        public decimal UnitPrice
        {
            get => _unitPrice;
            set { _unitPrice = value; OnPropertyChanged(nameof(UnitPrice)); OnPropertyChanged(nameof(Subtotal)); }
        }
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(nameof(Quantity)); OnPropertyChanged(nameof(Subtotal)); }
        }
        public decimal Subtotal => UnitPrice * Quantity;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public partial class NewPurchaseWindow : Window
    {
        private ObservableCollection<PurchaseItemViewModel> _invoiceItems = new ObservableCollection<PurchaseItemViewModel>();

        public NewPurchaseWindow()
        {
            InitializeComponent();
            LoadSuppliers();
            InvoiceItemsDataGrid.ItemsSource = _invoiceItems;
            _invoiceItems.CollectionChanged += (s, e) => UpdateTotal();
        }

        private void LoadSuppliers()
        {
            using (var db = new DatabaseContext()) { SupplierComboBox.ItemsSource = db.Suppliers.ToList(); }
        }

        private void SearchProductTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                using (var db = new DatabaseContext())
                {
                    var product = db.Products.FirstOrDefault(p => p.ProductCode.ToLower() == SearchProductTextBox.Text.ToLower() || p.Name.ToLower().Contains(SearchProductTextBox.Text.ToLower()));
                    if (product != null)
                    {
                        var existingItem = _invoiceItems.FirstOrDefault(item => item.ProductId == product.Id);
                        if (existingItem != null) { existingItem.Quantity++; }
                        else
                        {
                            _invoiceItems.Add(new PurchaseItemViewModel
                            {
                                ProductId = product.Id,
                                ProductName = product.Name,
                                UnitPrice = product.PurchasePrice, // سعر الشراء الافتراضي
                                Quantity = 1
                            });
                        }
                        SearchProductTextBox.Clear();
                    }
                    else { MessageBox.Show("لم يتم العثور على المنتج."); }
                }
            }
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is PurchaseItemViewModel itemToRemove)
            {
                _invoiceItems.Remove(itemToRemove);
            }
        }

        private void UpdateTotal()
        {
            TotalAmountTextBlock.Text = $"{_invoiceItems.Sum(item => item.Subtotal):C}";
        }

        private void CompletePurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (SupplierComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(InvoiceNumberTextBox.Text) || !_invoiceItems.Any())
            {
                MessageBox.Show("يرجى اختيار مورد، إدخال رقم الفاتورة، وإضافة منتجات.", "بيانات غير مكتملة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // --- بداية التحديث: جلب إعدادات الحسابات الافتراضية ---
                    var companyInfo = db.CompanyInfos.FirstOrDefault();
                    if (companyInfo?.DefaultPurchasesAccountId == null || companyInfo?.DefaultAccountsPayableAccountId == null)
                    {
                        MessageBox.Show("يرجى تحديد حساب المشتريات وحساب الموردين الافتراضي في شاشة الإعدادات أولاً.", "إعدادات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    // --- نهاية التحديث ---

                    var purchase = new Purchase
                    {
                        InvoiceNumber = InvoiceNumberTextBox.Text,
                        PurchaseDate = DateTime.Now,
                        SupplierId = (int)SupplierComboBox.SelectedValue,
                        TotalAmount = _invoiceItems.Sum(item => item.Subtotal)
                    };
                    db.Purchases.Add(purchase);
                    db.SaveChanges();

                    foreach (var item in _invoiceItems)
                    {
                        db.PurchaseItems.Add(new PurchaseItem
                        {
                            PurchaseId = purchase.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        });

                        var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == item.ProductId);
                        if (inventory != null) { inventory.Quantity += item.Quantity; }
                        else { db.Inventories.Add(new Inventory { ProductId = item.ProductId, Quantity = item.Quantity }); }
                    }

                    // --- بداية التحديث: إنشاء القيد المحاسبي التلقائي ---
                    var journalVoucher = new JournalVoucher
                    {
                        VoucherNumber = $"PV-{purchase.InvoiceNumber}",
                        VoucherDate = purchase.PurchaseDate,
                        Description = $"قيد إثبات مشتريات الفاتورة رقم {purchase.InvoiceNumber}",
                        TotalDebit = purchase.TotalAmount,
                        TotalCredit = purchase.TotalAmount
                    };
                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultPurchasesAccountId.Value, Debit = purchase.TotalAmount, Credit = 0 });
                    journalVoucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultAccountsPayableAccountId.Value, Debit = 0, Credit = purchase.TotalAmount });
                    db.JournalVouchers.Add(journalVoucher);
                    // --- نهاية التحديث ---

                    db.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("تم تسجيل فاتورة الشراء والقيد المحاسبي بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشلت العملية: {ex.Message}", "خطأ فادح", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}