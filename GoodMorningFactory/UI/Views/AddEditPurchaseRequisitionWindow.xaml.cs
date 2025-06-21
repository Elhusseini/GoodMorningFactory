// UI/Views/AddEditPurchaseRequisitionWindow.xaml.cs
// *** الكود الكامل لنافذة إضافة وتعديل طلب شراء مع إضافة مُنشئ التعديل ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public class PurchaseRequisitionItemViewModel : INotifyPropertyChanged
    {
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public partial class AddEditPurchaseRequisitionWindow : Window
    {
        private int? _requisitionId;
        private ObservableCollection<PurchaseRequisitionItemViewModel> _items = new ObservableCollection<PurchaseRequisitionItemViewModel>();

        public AddEditPurchaseRequisitionWindow(int? requisitionId = null)
        {
            InitializeComponent();
            _requisitionId = requisitionId;
            ItemsDataGrid.ItemsSource = _items;

            if (_requisitionId.HasValue)
            {
                Title = "تعديل طلب شراء";
                LoadRequisitionData();
            }
            else
            {
                Title = "إنشاء طلب شراء جديد";
            }
        }

        private void LoadRequisitionData()
        {
            if (!_requisitionId.HasValue) return;

            using (var db = new DatabaseContext())
            {
                var requisition = db.PurchaseRequisitions.Include(pr => pr.PurchaseRequisitionItems).FirstOrDefault(pr => pr.Id == _requisitionId.Value);
                if (requisition != null)
                {
                    RequesterTextBox.Text = requisition.RequesterName;
                    DepartmentTextBox.Text = requisition.Department;
                    PurposeTextBox.Text = requisition.Purpose;
                    foreach (var item in requisition.PurchaseRequisitionItems)
                    {
                        _items.Add(new PurchaseRequisitionItemViewModel
                        {
                            Description = item.Description,
                            Quantity = item.Quantity,
                            UnitOfMeasure = item.UnitOfMeasure
                        });
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RequesterTextBox.Text) || string.IsNullOrWhiteSpace(DepartmentTextBox.Text) || !_items.Any())
            {
                MessageBox.Show("يرجى إدخال جميع البيانات المطلوبة وإضافة أصناف.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new DatabaseContext())
                {
                    PurchaseRequisition pr;
                    if (_requisitionId.HasValue)
                    {
                        pr = db.PurchaseRequisitions.Include(p => p.PurchaseRequisitionItems).FirstOrDefault(p => p.Id == _requisitionId.Value);
                        db.PurchaseRequisitionItems.RemoveRange(pr.PurchaseRequisitionItems);
                    }
                    else
                    {
                        pr = new PurchaseRequisition { RequisitionNumber = $"PR-{DateTime.Now:yyyyMMddHHmmss}" };
                        db.PurchaseRequisitions.Add(pr);
                    }

                    pr.RequisitionDate = DateTime.Today;
                    pr.RequesterName = RequesterTextBox.Text;
                    pr.Department = DepartmentTextBox.Text;
                    pr.Purpose = PurposeTextBox.Text;
                    pr.Status = RequisitionStatus.Draft;

                    foreach (var item in _items.Where(i => !string.IsNullOrWhiteSpace(i.Description) && i.Quantity > 0))
                    {
                        pr.PurchaseRequisitionItems.Add(new PurchaseRequisitionItem
                        {
                            Description = item.Description,
                            Quantity = item.Quantity,
                            UnitOfMeasure = item.UnitOfMeasure
                        });
                    }

                    db.SaveChanges();
                    MessageBox.Show("تم حفظ طلب الشراء بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل إنشاء طلب الشراء: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
