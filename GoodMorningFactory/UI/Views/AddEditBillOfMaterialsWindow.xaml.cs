// UI/Views/AddEditBillOfMaterialsWindow.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public class BillOfMaterialsItemViewModel : INotifyPropertyChanged
    {
        public int RawMaterialId { get; set; }
        public string RawMaterialName { get; set; }
        public decimal Quantity { get; set; }
        public decimal ScrapPercentage { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public partial class AddEditBillOfMaterialsWindow : Window
    {
        private int? _bomId;
        private int? _sourceBomIdToCopy;
        private ObservableCollection<BillOfMaterialsItemViewModel> _items = new ObservableCollection<BillOfMaterialsItemViewModel>();

        public AddEditBillOfMaterialsWindow(int? bomId = null, int? sourceBomIdToCopy = null)
        {
            InitializeComponent();
            _bomId = bomId;
            _sourceBomIdToCopy = sourceBomIdToCopy;
            BomItemsDataGrid.ItemsSource = _items;
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            using (var db = new DatabaseContext())
            {
                FinishedGoodComboBox.ItemsSource = db.Products.Where(p => p.ProductType == ProductType.FinishedGood).ToList();
            }

            if (_bomId.HasValue) // وضع التعديل
            {
                Title = "تعديل قائمة مكونات";
                using (var db = new DatabaseContext())
                {
                    var bom = db.BillOfMaterials.Include(b => b.BillOfMaterialsItems).ThenInclude(i => i.RawMaterial)
                                .FirstOrDefault(b => b.Id == _bomId.Value);
                    if (bom != null)
                    {
                        FinishedGoodComboBox.SelectedValue = bom.FinishedGoodId;
                        DescriptionTextBox.Text = bom.Description;
                        foreach (var item in bom.BillOfMaterialsItems)
                        {
                            _items.Add(new BillOfMaterialsItemViewModel
                            {
                                RawMaterialId = item.RawMaterialId,
                                RawMaterialName = item.RawMaterial.Name,
                                Quantity = item.Quantity,
                                ScrapPercentage = item.ScrapPercentage
                            });
                        }
                    }
                }
            }
            else if (_sourceBomIdToCopy.HasValue) // وضع النسخ
            {
                Title = "نسخ قائمة مكونات";
                using (var db = new DatabaseContext())
                {
                    var bomToCopy = db.BillOfMaterials.Include(b => b.BillOfMaterialsItems).ThenInclude(i => i.RawMaterial)
                                     .FirstOrDefault(b => b.Id == _sourceBomIdToCopy.Value);
                    if (bomToCopy != null)
                    {
                        DescriptionTextBox.Text = $"نسخة من: {bomToCopy.Description}";
                        foreach (var item in bomToCopy.BillOfMaterialsItems)
                        {
                            _items.Add(new BillOfMaterialsItemViewModel
                            {
                                RawMaterialId = item.RawMaterialId,
                                RawMaterialName = item.RawMaterial.Name,
                                Quantity = item.Quantity,
                                ScrapPercentage = item.ScrapPercentage
                            });
                        }
                    }
                }
            }
            else // وضع الإضافة
            {
                Title = "إنشاء قائمة مكونات جديدة";
            }
        }

        // === بداية الإصلاح: دالة مركزية لإضافة المواد ===
        private void AddMaterial()
        {
            if (string.IsNullOrWhiteSpace(SearchMaterialTextBox.Text)) return;

            using (var db = new DatabaseContext())
            {
                var material = db.Products.FirstOrDefault(p => p.ProductType == ProductType.RawMaterial &&
                                                              (p.ProductCode.ToLower() == SearchMaterialTextBox.Text.ToLower() || p.Name.ToLower().Contains(SearchMaterialTextBox.Text.ToLower())));
                if (material != null)
                {
                    if (!_items.Any(i => i.RawMaterialId == material.Id))
                    {
                        _items.Add(new BillOfMaterialsItemViewModel { RawMaterialId = material.Id, RawMaterialName = material.Name, Quantity = 1, ScrapPercentage = 0 });
                    }
                    else
                    {
                        // يمكنك إضافة منطق لزيادة الكمية إذا كان البند موجوداً
                        var existingItem = _items.First(i => i.RawMaterialId == material.Id);
                        existingItem.Quantity++;
                    }
                    SearchMaterialTextBox.Clear();
                }
                else
                {
                    MessageBox.Show("لم يتم العثور على المادة الخام.", "بحث", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void SearchMaterialTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddMaterial();
                e.Handled = true; // منع الحدث من تفعيل زر الحفظ
            }
        }

        private void AddMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            AddMaterial();
        }
        // === نهاية الإصلاح ===

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is BillOfMaterialsItemViewModel item)
            {
                var result = MessageBox.Show($"سيتم حذف المادة: {item.RawMaterialName}. هل تريد المتابعة؟", "تأكيد الحذف", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.OK)
                {
                    _items.Remove(item);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (FinishedGoodComboBox.SelectedItem == null)
            {
                MessageBox.Show("يرجى اختيار المنتج النهائي.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DatabaseContext())
            {
                BillOfMaterials bom;
                if (_bomId.HasValue)
                {
                    bom = db.BillOfMaterials.Include(b => b.BillOfMaterialsItems).FirstOrDefault(b => b.Id == _bomId.Value);
                    db.BillOfMaterialsItems.RemoveRange(bom.BillOfMaterialsItems);
                }
                else
                {
                    bom = new BillOfMaterials();
                    db.BillOfMaterials.Add(bom);
                }

                bom.FinishedGoodId = (int)FinishedGoodComboBox.SelectedValue;
                bom.Description = DescriptionTextBox.Text;

                foreach (var item in _items)
                {
                    bom.BillOfMaterialsItems.Add(new BillOfMaterialsItem
                    {
                        RawMaterialId = item.RawMaterialId,
                        Quantity = item.Quantity,
                        ScrapPercentage = item.ScrapPercentage
                    });
                }

                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}
