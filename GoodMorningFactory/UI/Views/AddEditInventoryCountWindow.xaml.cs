// UI/Views/AddEditInventoryCountWindow.xaml.cs
// *** تحديث: إضافة إنشاء قيد محاسبي مجمع عند ترحيل أمر الجرد ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditInventoryCountWindow : Window
    {
        private readonly int? _inventoryCountId;
        private InventoryCount _inventoryCount;
        private ObservableCollection<InventoryCountItemViewModel> _items = new ObservableCollection<InventoryCountItemViewModel>();

        public AddEditInventoryCountWindow(int? inventoryCountId = null)
        {
            InitializeComponent();
            _inventoryCountId = inventoryCountId;
            CountItemsDataGrid.ItemsSource = _items;
            LoadInitialData();

            if (_inventoryCountId.HasValue)
            {
                LoadExistingCountData();
            }
            else
            {
                _inventoryCount = new InventoryCount();
                ReferenceTextBox.Text = $"IC-{DateTime.Now:yyyyMMddHHmmss}";
                CountDatePicker.SelectedDate = DateTime.Today;
                StatusTextBox.Text = "مخطط له";
            }
        }

        private void LoadInitialData()
        {
            using (var db = new DatabaseContext())
            {
                WarehouseComboBox.ItemsSource = db.Warehouses.Where(w => w.IsActive).ToList();
                UserComboBox.ItemsSource = db.Users.Where(u => u.IsActive).ToList();
                if (CurrentUserService.LoggedInUser != null)
                {
                    UserComboBox.SelectedValue = CurrentUserService.LoggedInUser.Id;
                }
            }
        }

        private void LoadExistingCountData()
        {
            using (var db = new DatabaseContext())
            {
                _inventoryCount = db.InventoryCounts
                    .Include(ic => ic.Items)
                        .ThenInclude(i => i.Product)
                    .Include(ic => ic.Items)
                        .ThenInclude(i => i.StorageLocation)
                    .FirstOrDefault(ic => ic.Id == _inventoryCountId.Value);

                if (_inventoryCount == null)
                {
                    MessageBox.Show("لم يتم العثور على أمر الجرد.", "خطأ");
                    this.Close();
                    return;
                }

                WindowTitle.Text = $"تفاصيل أمر الجرد: {_inventoryCount.CountReferenceNumber}";
                ReferenceTextBox.Text = _inventoryCount.CountReferenceNumber;
                CountDatePicker.SelectedDate = _inventoryCount.CountDate;
                WarehouseComboBox.SelectedValue = _inventoryCount.WarehouseId;
                UserComboBox.SelectedValue = _inventoryCount.ResponsibleUserId;
                StatusTextBox.Text = _inventoryCount.Status.ToString();
                NotesTextBox.Text = _inventoryCount.Notes;

                foreach (var item in _inventoryCount.Items)
                {
                    _items.Add(new InventoryCountItemViewModel
                    {
                        ProductId = item.ProductId,
                        ProductCode = item.Product.ProductCode,
                        ProductName = item.Product.Name,
                        StorageLocationId = item.StorageLocationId,
                        StorageLocationName = item.StorageLocation.Name,
                        SystemQuantity = item.SystemQuantity,
                        CountedQuantity = item.CountedQuantity
                    });
                }

                if (_inventoryCount.Status == InventoryCountStatus.Completed || _inventoryCount.Status == InventoryCountStatus.Cancelled)
                {
                    WarehouseComboBox.IsEnabled = false;
                    LoadProductsButton.IsEnabled = false;
                    CountItemsDataGrid.IsReadOnly = true;
                    SaveDraftButton.IsEnabled = false;
                    PostButton.IsEnabled = false;
                }
            }
        }

        private void WarehouseComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _items.Clear();
        }

        private void LoadProductsButton_Click(object sender, RoutedEventArgs e)
        {
            if (WarehouseComboBox.SelectedItem == null)
            {
                MessageBox.Show("يرجى اختيار المخزن أولاً.", "تنبيه");
                return;
            }

            _items.Clear();
            int warehouseId = (int)WarehouseComboBox.SelectedValue;

            using (var db = new DatabaseContext())
            {
                var productsInWarehouse = db.Inventories
                    .Include(i => i.Product)
                    .Include(i => i.StorageLocation)
                    .Where(i => i.StorageLocation.WarehouseId == warehouseId)
                    .ToList();

                foreach (var inv in productsInWarehouse)
                {
                    _items.Add(new InventoryCountItemViewModel
                    {
                        ProductId = inv.ProductId,
                        ProductCode = inv.Product.ProductCode,
                        ProductName = inv.Product.Name,
                        StorageLocationId = inv.StorageLocationId,
                        StorageLocationName = inv.StorageLocation.Name,
                        SystemQuantity = inv.Quantity,
                        CountedQuantity = inv.Quantity
                    });
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveInventoryCount(InventoryCountStatus.InProgress);
        }

        private void PostButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("هل أنت متأكد من ترحيل الفروقات؟ سيتم تحديث أرصدة المخزون وإنشاء قيد محاسبي، ولا يمكن التراجع عن هذه العملية.", "تأكيد الترحيل", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
            {
                return;
            }

            if (SaveInventoryCount(InventoryCountStatus.Completed))
            {
                using (var db = new DatabaseContext())
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var companyInfo = db.CompanyInfos.FirstOrDefault();
                        if (companyInfo?.DefaultInventoryAccountId == null || companyInfo?.DefaultInventoryAdjustmentAccountId == null)
                        {
                            throw new Exception("يرجى تحديد حساب 'المخزون' و 'تسوية المخزون' الافتراضي في شاشة الإعدادات أولاً.");
                        }

                        decimal totalAdjustmentValue = 0;

                        foreach (var item in _items)
                        {
                            if (item.Difference != 0)
                            {
                                var product = db.Products.Find(item.ProductId);
                                totalAdjustmentValue += item.Difference * product.AverageCost;

                                var inventory = db.Inventories.FirstOrDefault(i => i.ProductId == item.ProductId && i.StorageLocationId == item.StorageLocationId);
                                if (inventory != null)
                                {
                                    inventory.Quantity = item.CountedQuantity;
                                }

                                var movementType = item.Difference > 0 ? StockMovementType.AdjustmentIncrease : StockMovementType.AdjustmentDecrease;
                                db.StockMovements.Add(new StockMovement
                                {
                                    ProductId = item.ProductId,
                                    StorageLocationId = item.StorageLocationId,
                                    MovementDate = DateTime.Now,
                                    Quantity = Math.Abs(item.Difference),
                                    MovementType = movementType,
                                    ReferenceDocument = _inventoryCount.CountReferenceNumber,
                                    UnitCost = product.AverageCost,
                                    UserId = CurrentUserService.LoggedInUser?.Id
                                });
                            }
                        }

                        // إنشاء قيد محاسبي مجمع بصافي قيمة الفروقات
                        if (totalAdjustmentValue != 0)
                        {
                            var voucher = new JournalVoucher
                            {
                                VoucherNumber = $"JV-{_inventoryCount.CountReferenceNumber}",
                                VoucherDate = _inventoryCount.CountDate,
                                Description = $"تسوية فروقات الجرد لأمر الجرد رقم: {_inventoryCount.CountReferenceNumber}",
                                TotalDebit = Math.Abs(totalAdjustmentValue),
                                TotalCredit = Math.Abs(totalAdjustmentValue),
                                Status = VoucherStatus.Posted
                            };

                            if (totalAdjustmentValue > 0) // صافي زيادة في قيمة المخزون
                            {
                                voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Debit = totalAdjustmentValue, Credit = 0 });
                                voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAdjustmentAccountId.Value, Debit = 0, Credit = totalAdjustmentValue });
                            }
                            else // صافي نقص في قيمة المخزون
                            {
                                voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAdjustmentAccountId.Value, Debit = Math.Abs(totalAdjustmentValue), Credit = 0 });
                                voucher.JournalVoucherItems.Add(new JournalVoucherItem { AccountId = companyInfo.DefaultInventoryAccountId.Value, Debit = 0, Credit = Math.Abs(totalAdjustmentValue) });
                            }
                            db.JournalVouchers.Add(voucher);
                        }

                        db.SaveChanges();
                        transaction.Commit();
                        MessageBox.Show("تم ترحيل الفروقات وتحديث المخزون والقيود المحاسبية بنجاح.", "نجاح");
                        this.DialogResult = true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"فشل ترحيل الفروقات: {ex.Message}", "خطأ فادح");
                    }
                }
            }
        }

        private bool SaveInventoryCount(InventoryCountStatus status)
        {
            if (WarehouseComboBox.SelectedItem == null)
            {
                MessageBox.Show("يرجى اختيار المخزن.", "بيانات ناقصة");
                return false;
            }

            using (var db = new DatabaseContext())
            {
                if (_inventoryCount.Id == 0)
                {
                    db.InventoryCounts.Add(_inventoryCount);
                }
                else
                {
                    db.InventoryCounts.Attach(_inventoryCount);
                    db.Entry(_inventoryCount).State = EntityState.Modified;

                    var oldItems = db.InventoryCountItems.Where(i => i.InventoryCountId == _inventoryCount.Id);
                    db.InventoryCountItems.RemoveRange(oldItems);
                }

                _inventoryCount.CountReferenceNumber = ReferenceTextBox.Text;
                _inventoryCount.CountDate = CountDatePicker.SelectedDate ?? DateTime.Today;
                _inventoryCount.WarehouseId = (int)WarehouseComboBox.SelectedValue;
                _inventoryCount.ResponsibleUserId = (int?)UserComboBox.SelectedValue;
                _inventoryCount.Notes = NotesTextBox.Text;
                _inventoryCount.Status = status;

                foreach (var vm in _items)
                {
                    _inventoryCount.Items.Add(new InventoryCountItem
                    {
                        ProductId = vm.ProductId,
                        StorageLocationId = vm.StorageLocationId,
                        SystemQuantity = vm.SystemQuantity,
                        CountedQuantity = vm.CountedQuantity
                    });
                }

                try
                {
                    db.SaveChanges();
                    if (status != InventoryCountStatus.Completed)
                    {
                        MessageBox.Show("تم حفظ مسودة الجرد بنجاح.", "نجاح");
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل حفظ أمر الجرد: {ex.Message}", "خطأ");
                    return false;
                }
            }
        }
    }
}
