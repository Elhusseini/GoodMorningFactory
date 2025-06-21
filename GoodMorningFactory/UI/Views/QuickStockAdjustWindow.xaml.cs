// UI/Views/QuickStockAdjustWindow.xaml.cs
// *** تحديث: إضافة إنشاء قيد محاسبي تلقائي عند التعديل ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class QuickStockAdjustWindow : Window
    {
        private readonly int _productId;
        private readonly int _storageLocationId;
        private Inventory _inventory;

        public QuickStockAdjustWindow(int productId, int storageLocationId)
        {
            InitializeComponent();
            _productId = productId;
            _storageLocationId = storageLocationId;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    _inventory = db.Inventories
                        .Include(i => i.Product)
                        .Include(i => i.StorageLocation)
                        .FirstOrDefault(i => i.ProductId == _productId && i.StorageLocationId == _storageLocationId);

                    if (_inventory == null)
                    {
                        MessageBox.Show("لم يتم العثور على سجل المخزون المطلوب.", "خطأ");
                        this.Close();
                        return;
                    }

                    ProductNameTextBlock.Text = _inventory.Product.Name;
                    LocationTextBlock.Text = _inventory.StorageLocation.Name;
                    SystemQuantityTextBox.Text = _inventory.Quantity.ToString();
                    NewQuantityTextBox.Text = _inventory.Quantity.ToString();
                    NewQuantityTextBox.Focus();
                    NewQuantityTextBox.SelectAll();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ");
                this.Close();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(NewQuantityTextBox.Text, out int newQuantity) || newQuantity < 0)
            {
                MessageBox.Show("يرجى إدخال كمية رقمية صحيحة (أكبر من أو تساوي صفر).", "خطأ في الإدخال");
                return;
            }

            int difference = newQuantity - _inventory.Quantity;
            if (difference == 0)
            {
                MessageBox.Show("لم يتم تغيير الكمية.", "معلومة");
                this.DialogResult = false;
                return;
            }

            using (var db = new DatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // التحقق من وجود الحسابات الافتراضية
                    var companyInfo = db.CompanyInfos.FirstOrDefault();
                    if (companyInfo?.DefaultInventoryAccountId == null || companyInfo?.DefaultInventoryAdjustmentAccountId == null)
                    {
                        throw new Exception("يرجى تحديد حساب 'المخزون' و 'تسوية المخزون' الافتراضي في شاشة الإعدادات أولاً.");
                    }

                    // 1. تحديث رصيد المخزون
                    var inventoryToUpdate = db.Inventories.Include(i => i.Product).FirstOrDefault(i => i.Id == _inventory.Id);
                    if (inventoryToUpdate == null) throw new Exception("لم يتم العثور على سجل المخزون لتحديثه.");

                    inventoryToUpdate.Quantity = newQuantity;
                    inventoryToUpdate.LastUpdated = DateTime.Now;

                    // 2. إنشاء حركة مخزون
                    var movementType = difference > 0 ? StockMovementType.AdjustmentIncrease : StockMovementType.AdjustmentDecrease;
                    var reference = $"ADJ-QCK-{DateTime.Now:yyyyMMddHHmmss}";
                    decimal adjustmentValue = Math.Abs(difference) * inventoryToUpdate.Product.AverageCost;

                    db.StockMovements.Add(new StockMovement
                    {
                        ProductId = _productId,
                        StorageLocationId = _storageLocationId,
                        MovementDate = DateTime.Now,
                        MovementType = movementType,
                        Quantity = Math.Abs(difference),
                        UnitCost = inventoryToUpdate.Product.AverageCost,
                        ReferenceDocument = reference,
                        UserId = CurrentUserService.LoggedInUser?.Id
                    });

                    // 3. إنشاء قيد محاسبي بقيمة الفرق
                    if (adjustmentValue > 0)
                    {
                        var voucher = new JournalVoucher
                        {
                            VoucherNumber = $"JV-{reference}",
                            VoucherDate = DateTime.Today,
                            Description = $"تسوية مخزون سريعة للمنتج: {inventoryToUpdate.Product.Name}. السبب: {ReasonTextBox.Text}",
                            TotalDebit = adjustmentValue,
                            TotalCredit = adjustmentValue,
                            Status = VoucherStatus.Posted
                        };

                        if (difference > 0) // زيادة في المخزون
                        {
                            // مدين: حساب المخزون، دائن: حساب التسوية
                            voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Debit = adjustmentValue, Credit = 0 });
                            voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAdjustmentAccountId.Value, Debit = 0, Credit = adjustmentValue });
                        }
                        else // نقص في المخزون
                        {
                            // مدين: حساب التسوية، دائن: حساب المخزون
                            voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAdjustmentAccountId.Value, Debit = adjustmentValue, Credit = 0 });
                            voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Debit = 0, Credit = adjustmentValue });
                        }
                        db.JournalVouchers.Add(voucher);
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    MessageBox.Show("تم تحديث رصيد المخزون والقيد المحاسبي بنجاح.", "نجاح");
                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"فشل حفظ التعديل: {ex.Message}", "خطأ فادح");
                }
            }
        }
    }
}
