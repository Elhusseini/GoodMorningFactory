// UI/Views/AddEditPurchaseRequisitionWindow.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.Core.Services;
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
        public int? ProductId { get; set; }
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
            InitializeWindow();

            if (_requisitionId.HasValue)
            {
                Title = "تعديل طلب شراء";
                LoadRequisitionData();
            }
            else
            {
                Title = "إنشاء طلب شراء جديد";
                _items.Add(new PurchaseRequisitionItemViewModel());
            }
        }

        public AddEditPurchaseRequisitionWindow(int productId, decimal quantity)
        {
            InitializeComponent();
            InitializeWindow();
            Title = "إنشاء طلب شراء من MRP";

            using (var db = new DatabaseContext())
            {
                var product = db.Products.Include(p => p.UnitOfMeasure).FirstOrDefault(p => p.Id == productId);
                if (product != null)
                {
                    _items.Add(new PurchaseRequisitionItemViewModel
                    {
                        ProductId = product.Id,
                        Description = product.Name,
                        Quantity = quantity,
                        UnitOfMeasure = product.UnitOfMeasure?.Name
                    });
                }
            }
        }

        private void InitializeWindow()
        {
            ItemsDataGrid.ItemsSource = _items;

            using (var db = new DatabaseContext())
            {
                ProductColumn.ItemsSource = db.Products.ToList();
                RequesterComboBox.ItemsSource = db.Users.Where(u => u.IsActive).ToList();
                DepartmentComboBox.ItemsSource = db.Departments.ToList();
            }

            if (CurrentUserService.LoggedInUser != null)
            {
                RequesterComboBox.SelectedValue = CurrentUserService.LoggedInUser.Username;
                if (CurrentUserService.LoggedInUser.DepartmentId != null)
                {
                    using (var db = new DatabaseContext())
                    {
                        var dept = db.Departments.FirstOrDefault(d => d.Id == CurrentUserService.LoggedInUser.DepartmentId);
                        if (dept != null)
                            DepartmentComboBox.SelectedValue = dept.Name;
                    }
                }
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
                    RequesterComboBox.SelectedValue = requisition.RequesterName;
                    DepartmentComboBox.SelectedValue = requisition.Department;
                    PurposeTextBox.Text = requisition.Purpose;

                    _items.Clear();
                    foreach (var item in requisition.PurchaseRequisitionItems)
                    {
                        _items.Add(new PurchaseRequisitionItemViewModel
                        {
                            ProductId = item.ProductId,
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
            if (RequesterComboBox.SelectedItem == null || DepartmentComboBox.SelectedItem == null || !_items.Any(i => i.Quantity > 0))
            {
                MessageBox.Show("يرجى اختيار مقدم الطلب والقسم وإضافة صنف واحد على الأقل بكمية صحيحة.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        if (pr == null)
                        {
                            MessageBox.Show("لم يتم العثور على طلب الشراء المطلوب تعديله.", "خطأ");
                            return;
                        }
                        db.PurchaseRequisitionItems.RemoveRange(pr.PurchaseRequisitionItems);
                    }
                    else
                    {
                        pr = new PurchaseRequisition { RequisitionNumber = $"PR-{DateTime.Now:yyyyMMddHHmmss}" };
                        db.PurchaseRequisitions.Add(pr);
                    }

                    pr.RequisitionDate = DateTime.Today;
                    pr.RequesterName = RequesterComboBox.SelectedValue?.ToString();
                    pr.Department = DepartmentComboBox.SelectedValue?.ToString();
                    pr.Purpose = PurposeTextBox.Text;
                    pr.Status = RequisitionStatus.Draft;

                    foreach (var item in _items.Where(i => i.Quantity > 0))
                    {
                        string description = item.Description;
                        if (string.IsNullOrWhiteSpace(description) && item.ProductId.HasValue)
                        {
                            description = db.Products.Find(item.ProductId.Value)?.Name;
                        }

                        if (string.IsNullOrWhiteSpace(description))
                        {
                            continue;
                        }

                        pr.PurchaseRequisitionItems.Add(new PurchaseRequisitionItem
                        {
                            ProductId = item.ProductId,
                            Description = description,
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
                string innerExceptionMessage = ex.InnerException != null ? ex.InnerException.Message : "لا توجد تفاصيل إضافية.";
                MessageBox.Show($"فشل إنشاء طلب الشراء: {ex.Message}\n\nالتفاصيل: {innerExceptionMessage}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
