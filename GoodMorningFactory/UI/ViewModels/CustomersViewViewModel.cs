using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using Microsoft.EntityFrameworkCore; // Required for DbUpdateException
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel لشاشة عرض العملاء.
    /// تم تحديثه ليقوم بتنسيق البيانات القادمة من الخدمات وتجهيزها للعرض.
    /// </summary>
    public class CustomersViewViewModel : BaseViewModel
    {
        // ... (Properties and other methods remain the same)
        #region الخدمات
        // ملاحظة: في بنية متقدمة، يفضل حقن هذه الخدمات بدلاً من إنشائها مباشرة.
        private readonly ICustomerService _customerService;
        private readonly IFilterService _filterService;
        #endregion

        #region الحقول الخاصة بالترقيم
        private int _currentPage = 1;
        private readonly int _pageSize = 15;
        private int _totalItems = 0;
        #endregion

        #region الخصائص العامة

        private ObservableCollection<CustomerViewModel> _customers;
        public ObservableCollection<CustomerViewModel> Customers
        {
            get => _customers;
            set { _customers = value; OnPropertyChanged(); }
        }

        private CustomerViewModel _selectedCustomer;
        public CustomerViewModel SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                // تأخير البحث لتجنب استدعاءات متعددة أثناء الكتابة
                // هذا الجزء يمكن تحسينه باستخدام Reactive extensions أو Task.Delay
                ResetAndLoad();
            }
        }

        private string _pageInfo;
        public string PageInfo
        {
            get => _pageInfo;
            set { _pageInfo = value; OnPropertyChanged(); }
        }

        public ObservableCollection<FilterItem<bool?>> StatusFilters { get; set; }
        private FilterItem<bool?> _selectedStatusFilter;
        public FilterItem<bool?> SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set { _selectedStatusFilter = value; OnPropertyChanged(); ResetAndLoad(); }
        }
        #endregion

        #region الأوامر
        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand ViewStatementCommand { get; }
        public ICommand ExportToCsvCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand RefreshCommand { get; }
        #endregion

        public CustomersViewViewModel()
        {
            _customerService = new CustomerService();
            _filterService = new FilterService();

            AddCustomerCommand = new RelayCommand(AddCustomer, _ => PermissionsService.CanAccess("Customers.Create"));
            EditCustomerCommand = new RelayCommand(EditCustomer, CanActOnCustomerAndUpdate);
            DeleteCustomerCommand = new RelayCommand(DeleteCustomer, CanActOnCustomerAndDelete);
            ViewStatementCommand = new RelayCommand(ViewStatement, CanActOnCustomer);
            ExportToCsvCommand = new RelayCommand(ExportToCsv, _ => PermissionsService.CanAccess("Reports.Export"));
            NextPageCommand = new RelayCommand(GoToNextPage, _ => _currentPage < GetTotalPages());
            PreviousPageCommand = new RelayCommand(GoToPreviousPage, _ => _currentPage > 1);
            RefreshCommand = new RelayCommand(async _ => await LoadCustomersAsync());

            Initialize();
        }

        private async void Initialize()
        {
            LoadFilters();
            await LoadCustomersAsync();
        }

        #region دوال تنفيذ الأوامر والمنطق
        private void LoadFilters()
        {
            StatusFilters = new ObservableCollection<FilterItem<bool?>>(_filterService.GetStatusFilters());
            _selectedStatusFilter = StatusFilters.First();
            OnPropertyChanged(nameof(StatusFilters));
            OnPropertyChanged(nameof(SelectedStatusFilter));
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                // 1. إنشاء معايير البحث
                var criteria = new CustomerFilterCriteria
                {
                    Page = _currentPage,
                    PageSize = _pageSize,
                    SearchText = this.SearchText,
                    IsActive = this.SelectedStatusFilter?.Value
                };

                // 2. جلب بيانات العملاء الأساسية (Models)
                var result = await _customerService.GetCustomersAsync(criteria);
                var customersFromDb = result.Items;
                _totalItems = result.TotalCount;

                // 3. جلب أرصدة هؤلاء العملاء
                var customerIds = customersFromDb.Select(c => c.Id);
                var balances = await _customerService.GetCustomerBalancesAsync(customerIds);

                // 4. تحويل (Mapping) من Model إلى ViewModel وتعيين الرصيد
                var customerViewModels = customersFromDb.Select(customer => new CustomerViewModel
                {
                    Id = customer.Id,
                    CustomerCode = customer.CustomerCode,
                    CustomerName = customer.CustomerName,
                    ContactPerson = customer.ContactPerson,
                    PhoneNumber = customer.PhoneNumber,
                    CreditLimit = customer.CreditLimit,
                    IsActive = customer.IsActive,
                    // تعيين الرصيد من القاموس الذي تم جلبه
                    CurrentBalance = balances.ContainsKey(customer.Id) ? balances[customer.Id] : 0
                });

                Customers = new ObservableCollection<CustomerViewModel>(customerViewModels);
                UpdatePageInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل العملاء: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddCustomer(object parameter)
        {
            var addWindow = new AddEditCustomerWindow(null);
            if (addWindow.ShowDialog() == true)
            {
                await ResetAndLoad();
            }
        }

        private async void EditCustomer(object parameter)
        {
            if (parameter is CustomerViewModel customerVM)
            {
                // استدعاء الخدمة لجلب الكائن الأصلي الكامل قبل التعديل
                var originalCustomer = await _customerService.GetCustomerByIdAsync(customerVM.Id);
                if (originalCustomer != null)
                {
                    var editWindow = new AddEditCustomerWindow(originalCustomer);
                    if (editWindow.ShowDialog() == true)
                    {
                        await LoadCustomersAsync();
                    }
                }
            }
        }

        /// <summary>
        /// *** بداية الإصلاح ***
        /// تم تحسين معالجة الأخطاء لعرض رسائل مخصصة وواضحة للمستخدم.
        /// </summary>
        private async void DeleteCustomer(object parameter)
        {
            if (parameter is CustomerViewModel customerVM &&
                MessageBox.Show($"هل أنت متأكد من حذف العميل '{customerVM.CustomerName}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    await _customerService.DeleteCustomerAsync(customerVM.Id);
                    await LoadCustomersAsync(); // تحديث القائمة بعد الحذف الناجح
                }
                catch (InvalidOperationException ex) // التقاط الخطأ المخصص من الخدمة
                {
                    MessageBox.Show(ex.Message, "عملية مرفوضة", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (DbUpdateException) // التقاط أخطاء قاعدة البيانات العامة
                {
                    MessageBox.Show("لا يمكن حذف هذا العميل لوجود سجلات أخرى مرتبطة به في النظام.", "خطأ في الحذف", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex) // التقاط أي أخطاء أخرى غير متوقعة
                {
                    MessageBox.Show($"حدث خطأ غير متوقع: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        /// *** نهاية الإصلاح ***

        private void ViewStatement(object parameter)
        {
            if (parameter is CustomerViewModel customerVM)
            {
                var statementWindow = new CustomerStatementWindow(customerVM.Id);
                statementWindow.Show();
            }
        }

        private void ExportToCsv(object parameter)
        {
            if (Customers == null || !Customers.Any())
            {
                MessageBox.Show("لا توجد بيانات لتصديرها.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV (Comma delimited) (*.csv)|*.csv",
                FileName = $"Customers_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("CustomerCode,CustomerName,ContactPerson,PhoneNumber,CreditLimit,CurrentBalance,IsActive");

                    foreach (var customer in Customers)
                    {
                        var line = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",{4},{5},{6}",
                            customer.CustomerCode, customer.CustomerName, customer.ContactPerson,
                            customer.PhoneNumber, customer.CreditLimit, customer.CurrentBalance, customer.IsActive);
                        sb.AppendLine(line);
                    }

                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("تم تصدير البيانات بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل تصدير الملف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanActOnCustomer(object parameter) => parameter is CustomerViewModel;
        private bool CanActOnCustomerAndUpdate(object parameter) => CanActOnCustomer(parameter) && PermissionsService.CanAccess("Customers.Update");
        private bool CanActOnCustomerAndDelete(object parameter) => CanActOnCustomer(parameter) && PermissionsService.CanAccess("Customers.Delete");
        #endregion

        #region دوال مساعدة للترقيم
        private async Task ResetAndLoad()
        {
            _currentPage = 1;
            await LoadCustomersAsync();
        }

        private async void GoToNextPage(object parameter)
        {
            if (_currentPage < GetTotalPages())
            {
                _currentPage++;
                await LoadCustomersAsync();
            }
        }

        private async void GoToPreviousPage(object parameter)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadCustomersAsync();
            }
        }

        private int GetTotalPages()
        {
            if (_totalItems == 0) return 1;
            return (int)Math.Ceiling((double)_totalItems / _pageSize);
        }

        private void UpdatePageInfo()
        {
            PageInfo = $"الصفحة {_currentPage} من {GetTotalPages()} (إجمالي العملاء: {_totalItems})";
        }
        #endregion
    }
}
