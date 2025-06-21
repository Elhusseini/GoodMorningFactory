// GoodMorningFactory/UI/ViewModels/AddEditInventoryCountViewModel.cs
using GoodMorningFactory.Core.Helpers;
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditInventoryCountViewModel : BaseViewModel
    {
        private readonly IInventoryCountService _inventoryCountService;
        private readonly int? _inventoryCountId;
        private InventoryCount _inventoryCount;

        #region الخصائص (Properties)
        private string _windowTitle;
        public string WindowTitle { get => _windowTitle; set { _windowTitle = value; OnPropertyChanged(); } }

        private string _referenceNumber;
        public string ReferenceNumber { get => _referenceNumber; set { _referenceNumber = value; OnPropertyChanged(); } }

        private DateTime _countDate;
        public DateTime CountDate { get => _countDate; set { _countDate = value; OnPropertyChanged(); } }

        private string _statusText;
        public string StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged(); } }

        private string _notes;
        public string Notes { get => _notes; set { _notes = value; OnPropertyChanged(); } }

        public List<Warehouse> Warehouses { get; private set; }
        public List<User> Users { get; private set; }

        private Warehouse _selectedWarehouse;
        public Warehouse SelectedWarehouse
        {
            get => _selectedWarehouse;
            set
            {
                _selectedWarehouse = value;
                OnPropertyChanged();
                if (Items != null) Items.Clear();
            }
        }

        private User _selectedUser;
        public User SelectedUser { get => _selectedUser; set { _selectedUser = value; OnPropertyChanged(); } }

        public ObservableCollection<InventoryCountItemViewModel> Items { get; set; }

        private bool _isReadOnly;
        public bool IsReadOnly { get => _isReadOnly; set { _isReadOnly = value; OnPropertyChanged(); } }
        #endregion

        #region الأوامر (Commands)
        public ICommand LoadProductsCommand { get; }
        public ICommand SaveDraftCommand { get; }
        public ICommand PostCommand { get; }
        #endregion

        public AddEditInventoryCountViewModel()
        {
            WindowTitle = "إضافة / تعديل أمر جرد";
            Items = new ObservableCollection<InventoryCountItemViewModel>();
        }

        public AddEditInventoryCountViewModel(int? inventoryCountId = null)
        {
            _inventoryCountService = new InventoryCountService();
            _inventoryCountId = inventoryCountId;
            Items = new ObservableCollection<InventoryCountItemViewModel>();

            LoadProductsCommand = new RelayCommand(ExecuteLoadProducts, CanExecuteLoadProducts);
            SaveDraftCommand = new RelayCommand(ExecuteSaveDraft, _ => !IsReadOnly);
            PostCommand = new RelayCommand(ExecutePost, _ => !IsReadOnly);

            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                var dto = await _inventoryCountService.GetInitialDataForCountWindowAsync(_inventoryCountId);

                Warehouses = dto.Warehouses;
                Users = dto.Users;
                OnPropertyChanged(nameof(Warehouses));
                OnPropertyChanged(nameof(Users));

                _inventoryCount = dto.InventoryCount;
                if (_inventoryCount != null)
                {
                    ReferenceNumber = _inventoryCount.CountReferenceNumber;
                    CountDate = _inventoryCount.CountDate;
                    Notes = _inventoryCount.Notes;
                    StatusText = _inventoryCount.Status.GetDescription();

                    SelectedWarehouse = Warehouses.FirstOrDefault(w => w.Id == _inventoryCount.WarehouseId);
                    SelectedUser = Users.FirstOrDefault(u => u.Id == _inventoryCount.ResponsibleUserId);

                    Items = new ObservableCollection<InventoryCountItemViewModel>(dto.Items);
                    OnPropertyChanged(nameof(Items));

                    WindowTitle = _inventoryCountId.HasValue ? $"تفاصيل أمر الجرد: {_inventoryCount.CountReferenceNumber}" : "إضافة أمر جرد جديد";
                    IsReadOnly = _inventoryCount.Status == InventoryCountStatus.Completed || _inventoryCount.Status == InventoryCountStatus.Cancelled;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}", "خطأ");
            }
        }

        private async void ExecuteLoadProducts(object parameter)
        {
            if (SelectedWarehouse == null)
            {
                MessageBox.Show("يرجى اختيار المخزن أولاً.", "تنبيه");
                return;
            }

            Items.Clear();
            var products = await _inventoryCountService.GetProductsForWarehouseAsync(SelectedWarehouse.Id);
            foreach (var product in products)
            {
                Items.Add(product);
            }
        }

        private bool CanExecuteLoadProducts(object parameter)
        {
            return SelectedWarehouse != null && !IsReadOnly;
        }

        private async void ExecuteSaveDraft(object parameter)
        {
            await SaveCount(InventoryCountStatus.InProgress, parameter as Window);
        }

        private async void ExecutePost(object parameter)
        {
            var result = MessageBox.Show("هل أنت متأكد من ترحيل الفروقات؟ سيتم تحديث أرصدة المخزون وإنشاء قيد محاسبي، ولا يمكن التراجع عن هذه العملية.", "تأكيد الترحيل", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                if (await SaveCount(InventoryCountStatus.Completed, null))
                {
                    try
                    {
                        await _inventoryCountService.PostInventoryCountAsync(_inventoryCount.Id);
                        MessageBox.Show("تم ترحيل الفروقات وتحديث المخزون والقيود المحاسبية بنجاح.", "نجاح");

                        // ---  بداية التعديل الرئيسي  ---
                        // إخبار الواجهة الرئيسية بأن العملية نجحت لتقوم بالتحديث
                        if (parameter is Window window)
                        {
                            window.DialogResult = true;
                            window.Close();
                        }
                        // ---  نهاية التعديل الرئيسي  ---
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"فشل ترحيل الفروقات: {ex.Message}", "خطأ فادح");
                        await SaveCount(InventoryCountStatus.InProgress, null);
                    }
                }
            }
        }

        private async Task<bool> SaveCount(InventoryCountStatus status, Window window)
        {
            if (SelectedWarehouse == null)
            {
                MessageBox.Show("يرجى اختيار المخزن.", "بيانات ناقصة");
                return false;
            }

            _inventoryCount.CountDate = this.CountDate;
            _inventoryCount.WarehouseId = this.SelectedWarehouse.Id;
            _inventoryCount.ResponsibleUserId = this.SelectedUser?.Id;
            _inventoryCount.Notes = this.Notes ?? string.Empty; // ضمان عدم إرسال قيمة null
            _inventoryCount.Status = status;

            try
            {
                int savedId = await _inventoryCountService.SaveInventoryCountAsync(_inventoryCount, Items.ToList());
                _inventoryCount.Id = savedId;

                if (window != null)
                {
                    MessageBox.Show("تم حفظ أمر الجرد بنجاح.", "نجاح");
                    window.DialogResult = true;
                    window.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"فشل حفظ أمر الجرد: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\n--- التفاصيل الداخلية ---\n{ex.InnerException.Message}";
                }
                MessageBox.Show(errorMessage, "خطأ فادح");
                return false;
            }
        }
    }
}