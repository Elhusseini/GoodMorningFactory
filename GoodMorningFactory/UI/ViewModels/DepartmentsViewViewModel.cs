// GoodMorningFactory/UI/ViewModels/DepartmentsViewViewModel.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel الرئيسي لواجهة عرض الأقسام.
    /// يحتوي على كل الخصائص (Properties) والأوامر (Commands) التي تحتاجها الواجهة.
    /// </summary>
    public class DepartmentsViewViewModel : BaseViewModel
    {
        private readonly IDepartmentService _departmentService;

        #region الخصائص (Properties)

        // مجموعة الأقسام التي سيتم ربطها بالـ DataGrid
        private ObservableCollection<DepartmentViewModel> _departments;
        public ObservableCollection<DepartmentViewModel> Departments
        {
            get => _departments;
            set { _departments = value; OnPropertyChanged(); }
        }

        // القسم المحدد حالياً في الـ DataGrid
        private DepartmentViewModel _selectedDepartment;
        public DepartmentViewModel SelectedDepartment
        {
            get => _selectedDepartment;
            set { _selectedDepartment = value; OnPropertyChanged(); }
        }

        // النص المستخدم في مربع البحث
        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ResetAndLoad(); }
        }

        // معلومات الصفحة الحالية (مثال: "الصفحة 1 من 5")
        private string _pageInfo;
        public string PageInfo
        {
            get => _pageInfo;
            set { _pageInfo = value; OnPropertyChanged(); }
        }

        // متغيرات داخلية لإدارة الترقيم
        private int _currentPage = 1;
        private readonly int _pageSize = 15;
        private int _totalItems = 0;
        #endregion

        #region الأوامر (Commands)
        public ICommand AddDepartmentCommand { get; }
        public ICommand EditDepartmentCommand { get; }
        public ICommand DeleteDepartmentCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        #endregion

        public DepartmentsViewViewModel()
        {
            // إنشاء نسخة من الخدمة
            _departmentService = new DepartmentService();

            // ربط الأوامر بالوظائف الخاصة بها
            AddDepartmentCommand = new RelayCommand(AddDepartment);
            EditDepartmentCommand = new RelayCommand(EditDepartment, CanExecuteAction);
            DeleteDepartmentCommand = new RelayCommand(DeleteDepartment, CanExecuteAction);
            RefreshCommand = new RelayCommand(async _ => await LoadDepartmentsAsync());
            NextPageCommand = new RelayCommand(GoToNextPage, CanGoToNextPage);
            PreviousPageCommand = new RelayCommand(GoToPreviousPage, CanGoToPreviousPage);

            // تحميل البيانات عند إنشاء الـ ViewModel لأول مرة
            LoadDepartmentsAsync();
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var criteria = new DepartmentFilterCriteria
                {
                    Page = _currentPage,
                    PageSize = _pageSize,
                    SearchText = this.SearchText
                };

                var result = await _departmentService.GetDepartmentsAsync(criteria);
                Departments = new ObservableCollection<DepartmentViewModel>(result.Items);
                _totalItems = result.TotalCount;
                UpdatePageInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الأقسام: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region وظائف الأوامر

        private void AddDepartment(object parameter)
        {
            var addWindow = new AddEditDepartmentWindow();
            // إذا تم إغلاق النافذة بنجاح (بالضغط على حفظ)
            if (addWindow.ShowDialog() == true)
            {
                ResetAndLoad(); // إعادة تحميل البيانات
            }
        }

        private void EditDepartment(object parameter)
        {
            if (parameter is DepartmentViewModel department)
            {
                var editWindow = new AddEditDepartmentWindow(department.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDepartmentsAsync(); // تحديث القائمة
                }
            }
        }

        private async void DeleteDepartment(object parameter)
        {
            if (parameter is DepartmentViewModel department)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف القسم '{department.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                try
                {
                    await _departmentService.DeleteDepartmentAsync(department.Id);
                    await LoadDepartmentsAsync(); // تحديث القائمة بعد الحذف
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "خطأ في الحذف", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // وظيفة للتحقق من إمكانية تنفيذ أوامر التعديل والحذف (يجب تحديد قسم أولاً)
        private bool CanExecuteAction(object parameter) => parameter is DepartmentViewModel;

        #endregion

        #region وظائف مساعدة للترقيم

        // إعادة التعيين للصفحة الأولى ثم التحميل
        private async void ResetAndLoad()
        {
            _currentPage = 1;
            await LoadDepartmentsAsync();
        }

        private async void GoToNextPage(object parameter)
        {
            _currentPage++;
            await LoadDepartmentsAsync();
        }

        private async void GoToPreviousPage(object parameter)
        {
            _currentPage--;
            await LoadDepartmentsAsync();
        }

        private bool CanGoToNextPage(object parameter) => _currentPage < GetTotalPages();
        private bool CanGoToPreviousPage(object parameter) => _currentPage > 1;

        private int GetTotalPages()
        {
            if (_totalItems == 0) return 1;
            return (int)Math.Ceiling((double)_totalItems / _pageSize);
        }

        private void UpdatePageInfo()
        {
            PageInfo = $"الصفحة {_currentPage} من {GetTotalPages()} (إجمالي السجلات: {_totalItems})";
        }
        #endregion
    }
}
