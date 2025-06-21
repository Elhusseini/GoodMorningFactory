using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// نموذج العرض الرئيسي للتطبيق (MainViewModel)
    /// يدير المنطق البرمجي للنافذة الرئيسية وواجهة المستخدم الرئيسية
    /// يتبع نمط MVVM لفصل المنطق البرمجي عن واجهة المستخدم
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private string _currentUserName;

        /// <summary>
        /// اسم المستخدم الحالي المسجل دخوله
        /// يتم تحديثه تلقائياً عند تغيير المستخدم
        /// </summary>
        public string CurrentUserName
        {
            get => _currentUserName;
            set
            {
                _currentUserName = value;
                OnPropertyChanged(nameof(CurrentUserName));
            }
        }

        #region Commands
        /// <summary>
        /// مجموعة الأوامر المتاحة في الواجهة الرئيسية
        /// تستخدم نمط Command لربط أحداث واجهة المستخدم بالمنطق البرمجي
        /// </summary>
        public ICommand NavigateCommand { get; } // أمر التنقل بين الصفحات
        public ICommand LogoutCommand { get; }   // أمر تسجيل الخروج
        public ICommand ExitCommand { get; }     // أمر إغلاق التطبيق
        public ICommand ShowAboutWindowCommand { get; } // أمر عرض نافذة "حول البرنامج"
        public ICommand AdjustStockCommand { get; }    // أمر تعديل المخزون
        #endregion

        #region Permissions Properties
        /// <summary>
        /// خصائص الصلاحيات التي تحدد ما يمكن للمستخدم الوصول إليه
        /// يتم تحديثها عند تسجيل الدخول وتغيير المستخدم
        /// تستخدم في واجهة المستخدم لإخفاء/إظهار العناصر حسب صلاحيات المستخدم
        /// </summary>
        public bool CanViewMainData { get; private set; }         // صلاحية عرض البيانات الرئيسية
        public bool CanViewCrm { get; private set; }             // صلاحية عرض إدارة علاقات العملاء
        public bool CanViewSales { get; private set; }           // صلاحية عرض المبيعات
        public bool CanViewPurchases { get; private set; }       // صلاحية عرض المشتريات
        public bool CanViewManufacturing { get; private set; }   // صلاحية عرض التصنيع
        public bool CanViewQualityControl { get; private set; }  // صلاحية عرض مراقبة الجودة
        public bool CanViewInventory { get; private set; }       // صلاحية عرض المخزون
        public bool CanViewHR { get; private set; }             // صلاحية عرض الموارد البشرية
        public bool CanViewFinancials { get; private set; }     // صلاحية عرض الماليات
        public bool CanViewReports { get; private set; }        // صلاحية عرض التقارير
        public bool CanViewSettings { get; private set; }       // صلاحية عرض الإعدادات
        public bool CanViewSecurity { get; private set; }       // صلاحية عرض الأمان
        public bool CanViewLowStock { get; private set; }       // صلاحية عرض المخزون المنخفض
        public bool CanViewAuditTrail { get; private set; }     // صلاحية عرض سجل المراجعة
        public bool CanViewApprovalWorkflows { get; private set; } // صلاحية عرض سير عمل الموافقات
        public bool CanViewInventoryCounts { get; private set; }  // صلاحية عرض جرد المخزون
        #endregion

        /// <summary>
        /// منشئ نموذج العرض الرئيسي
        /// يقوم بتهيئة الأوامر وتحميل بيانات المستخدم وتطبيق الصلاحيات
        /// </summary>
        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            // تهيئة الأوامر وربطها بالدوال المنفذة
            NavigateCommand = new RelayCommand(ExecuteNavigate);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            ExitCommand = new RelayCommand(param => Application.Current.Shutdown());
            ShowAboutWindowCommand = new RelayCommand(param => new AboutWindow().ShowDialog());
            AdjustStockCommand = new RelayCommand(ExecuteAdjustStock);

            // تحميل بيانات المستخدم وتطبيق الصلاحيات
            LoadUserData();
            ApplyPermissions();
        }

        /// <summary>
        /// تنفيذ أمر التنقل بين الصفحات
        /// </summary>
        private void ExecuteNavigate(object parameter)
        {
            if (parameter is string viewName && !string.IsNullOrEmpty(viewName))
            {
                _navigationService.NavigateTo(viewName);
            }
        }

        /// <summary>
        /// تنفيذ أمر تعديل المخزون
        /// يفتح نافذة تعديل المخزون كحوار منبثق
        /// </summary>
        private void ExecuteAdjustStock(object parameter)
        {
            var adjustStockWindow = new AdjustStockWindow();
            adjustStockWindow.ShowDialog();
        }

        /// <summary>
        /// تنفيذ أمر تسجيل الخروج
        /// يعرض نافذة تسجيل الدخول ويعيد تحميل بيانات المستخدم عند نجاح تسجيل الدخول
        /// </summary>
        private void ExecuteLogout(object parameter)
        {
            var loginWindow = new LoginWindow();
            var result = loginWindow.ShowDialog();

            if (result == true)
            {
                LoadUserData();
                ApplyPermissions();
                _navigationService.NavigateTo("Dashboard");
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// تحميل بيانات المستخدم الحالي
        /// يتحقق من وجود مستخدم مسجل دخوله ويحدث اسم المستخدم في الواجهة
        /// </summary>
        private void LoadUserData()
        {
            if (CurrentUserService.LoggedInUser != null)
            {
                CurrentUserName = $"المستخدم: {CurrentUserService.LoggedInUser.Username}";
            }
            else
            {
                MessageBox.Show("فشل تحميل بيانات المستخدم. سيتم إغلاق البرنامج.");
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// تطبيق صلاحيات المستخدم
        /// يتحقق من كل صلاحية ويحدث خصائص الصلاحيات وفقاً لذلك
        /// تؤثر هذه الخصائص على ظهور/إخفاء عناصر واجهة المستخدم
        /// </summary>
        private void ApplyPermissions()
        {
            // التحقق من كل صلاحية باستخدام خدمة الصلاحيات
            CanViewMainData = PermissionsService.CanAccess("MainData.View");
            CanViewCrm = PermissionsService.CanAccess("CRM.View");
            CanViewSales = PermissionsService.CanAccess("Sales.View");
            CanViewPurchases = PermissionsService.CanAccess("Purchases.View");
            CanViewManufacturing = PermissionsService.CanAccess("Manufacturing.View");
            CanViewQualityControl = PermissionsService.CanAccess("QualityControl.View");
            CanViewInventory = PermissionsService.CanAccess("Inventory.View");
            CanViewHR = PermissionsService.CanAccess("HR.View");
            CanViewFinancials = PermissionsService.CanAccess("Financials.View");
            CanViewReports = PermissionsService.CanAccess("Reports.View");
            CanViewSettings = PermissionsService.CanAccess("Settings.View");
            CanViewSecurity = PermissionsService.CanAccess("Security.View");
            CanViewLowStock = PermissionsService.CanAccess("Inventory.LowStock.View");
            CanViewAuditTrail = PermissionsService.CanAccess("Admin.AuditTrail.View");
            CanViewApprovalWorkflows = PermissionsService.CanAccess("Admin.ApprovalWorkflows.Manage");
            CanViewInventoryCounts = PermissionsService.CanAccess("Inventory.Counts.View");

            // إخطار الواجهة بتغيير جميع الخصائص
            OnPropertyChanged(string.Empty);
        }
    }
}