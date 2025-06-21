// GoodMorningFactory/UI/ViewModels/AddEditPurchaseRequisitionViewModel.cs
// *** الكود الكامل والنهائي - تم إصلاح منطق إضافة الأسطر بالكامل ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditPurchaseRequisitionViewModel : BaseViewModel
    {
        private readonly IPurchaseRequisitionService _requisitionService;
        private readonly int? _requisitionId;
        private PurchaseRequisition _model;

        #region Properties
        public string Title { get; private set; }
        public List<User> AllUsers { get; private set; }
        public List<Department> AllDepartments { get; private set; }
        public List<Product> AllProducts { get; private set; }
        public List<UnitOfMeasure> AllUnitsOfMeasure { get; private set; }
        public ObservableCollection<PurchaseRequisitionItemViewModel> Items { get; set; }

        private string _selectedRequesterName;
        public string SelectedRequesterName { get => _selectedRequesterName; set { _selectedRequesterName = value; OnPropertyChanged(); } }
        private string _selectedDepartmentName;
        public string SelectedDepartmentName { get => _selectedDepartmentName; set { _selectedDepartmentName = value; OnPropertyChanged(); } }
        private string _purpose;
        public string Purpose { get => _purpose; set { _purpose = value; OnPropertyChanged(); } }
        #endregion

        public RelayCommand SaveCommand { get; }

        public AddEditPurchaseRequisitionViewModel(int? requisitionId = null)
        {
            _requisitionId = requisitionId;
            _requisitionService = new PurchaseRequisitionService();
            Items = new ObservableCollection<PurchaseRequisitionItemViewModel>();
            Items.CollectionChanged += Items_CollectionChanged;
            SaveCommand = new RelayCommand(Save, CanSave);
            LoadDataAsync();
        }

        // تم الإبقاء على هذا المُنشئ لحالات التكامل المستقبلية مثل MRP
        public AddEditPurchaseRequisitionViewModel(int productId, decimal quantity)
        {
            _requisitionService = new PurchaseRequisitionService();
            Items = new ObservableCollection<PurchaseRequisitionItemViewModel>();
            Items.CollectionChanged += Items_CollectionChanged;
            SaveCommand = new RelayCommand(Save, CanSave);
            LoadDataForMRPAsync(productId, quantity);
        }

        private async void LoadDataAsync()
        {
            using (var db = new DatabaseContext())
            {
                AllUsers = await db.Users.Where(u => u.IsActive).ToListAsync();
                AllDepartments = await db.Departments.ToListAsync();
                AllProducts = await db.Products.Include(p => p.UnitOfMeasure).ToListAsync();
                AllUnitsOfMeasure = await db.UnitsOfMeasure.ToListAsync();
                OnPropertyChanged(nameof(AllUsers));
                OnPropertyChanged(nameof(AllDepartments));
                OnPropertyChanged(nameof(AllProducts));
                OnPropertyChanged(nameof(AllUnitsOfMeasure));
            }

            if (_requisitionId.HasValue) // وضع التعديل
            {
                Title = "تعديل طلب شراء";
                _model = await _requisitionService.GetRequisitionByIdAsync(_requisitionId.Value);
                if (_model == null) return;

                SelectedRequesterName = _model.RequesterName;
                SelectedDepartmentName = _model.Department;
                Purpose = _model.Purpose;
                foreach (var item in _model.PurchaseRequisitionItems)
                {
                    Items.Add(new PurchaseRequisitionItemViewModel(item, AllProducts));
                }
            }
            else // وضع الإضافة
            {
                Title = "إنشاء طلب شراء جديد";
                _model = new PurchaseRequisition();
                if (CurrentUserService.LoggedInUser != null)
                {
                    SelectedRequesterName = CurrentUserService.LoggedInUser.Username;
                }
            }
            // أضف دائماً سطراً فارغاً في النهاية لبدء الإدخال أو لإضافة المزيد
            Items.Add(new PurchaseRequisitionItemViewModel(new PurchaseRequisitionItem(), AllProducts));
        }

        private async void LoadDataForMRPAsync(int productId, decimal quantity)
        {
            using (var db = new DatabaseContext())
            {
                AllUsers = await db.Users.Where(u => u.IsActive).ToListAsync();
                AllDepartments = await db.Departments.ToListAsync();
                AllProducts = await db.Products.Include(p => p.UnitOfMeasure).ToListAsync();
                AllUnitsOfMeasure = await db.UnitsOfMeasure.ToListAsync();
                OnPropertyChanged(nameof(AllUsers));
                OnPropertyChanged(nameof(AllDepartments));
                OnPropertyChanged(nameof(AllProducts));
                OnPropertyChanged(nameof(AllUnitsOfMeasure));
            }

            Title = "إنشاء طلب شراء من MRP";
            _model = new PurchaseRequisition();
            if (CurrentUserService.LoggedInUser != null)
            {
                SelectedRequesterName = CurrentUserService.LoggedInUser.Username;
            }

            var product = AllProducts.FirstOrDefault(p => p.Id == productId);
            if (product != null)
            {
                var newItem = new PurchaseRequisitionItem { ProductId = productId, Quantity = quantity };
                Items.Add(new PurchaseRequisitionItemViewModel(newItem, AllProducts));
            }
            // أضف سطراً فارغاً في النهاية
            Items.Add(new PurchaseRequisitionItemViewModel(new PurchaseRequisitionItem(), AllProducts));
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PurchaseRequisitionItemViewModel item in e.NewItems) item.PropertyChanged += Item_PropertyChanged;
            }
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PurchaseRequisitionItemViewModel item in e.OldItems) item.PropertyChanged -= Item_PropertyChanged;
            }
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PurchaseRequisitionItemViewModel.ProductId))
            {
                if (sender is PurchaseRequisitionItemViewModel changedItem && changedItem == Items.LastOrDefault())
                {
                    if (changedItem.ProductId.HasValue)
                    {
                        Items.Add(new PurchaseRequisitionItemViewModel(new PurchaseRequisitionItem(), AllProducts));
                    }
                }
            }
        }

        private bool CanSave(object obj) => !string.IsNullOrWhiteSpace(SelectedRequesterName) && !string.IsNullOrWhiteSpace(SelectedDepartmentName) && Items.Any(i => i.ProductId.HasValue && i.Quantity > 0);

        private async void Save(object parameter)
        {
            _model.RequesterName = SelectedRequesterName;
            _model.Department = SelectedDepartmentName;
            _model.Purpose = Purpose;
            _model.RequisitionDate = System.DateTime.Today;
            _model.Status = RequisitionStatus.Draft;

            _model.PurchaseRequisitionItems.Clear();
            foreach (var itemVM in Items.Where(i => i.ProductId.HasValue && i.Quantity > 0))
            {
                _model.PurchaseRequisitionItems.Add(itemVM.Model);
            }

            if (_model.Id == 0) _model.RequisitionNumber = $"PR-{System.DateTime.Now:yyyyMMddHHmmss}";

            await _requisitionService.SaveRequisitionAsync(_model);

            if (parameter is Window window) window.DialogResult = true;
        }
    }
}