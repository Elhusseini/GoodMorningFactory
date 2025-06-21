using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// النافذة الرئيسية للتطبيق
    /// تمثل الواجهة الأساسية التي تحتوي على قائمة التنقل وتعرض المحتوى الديناميكي
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// منشئ النافذة الرئيسية
        /// يقوم بتهيئة المكونات وإعداد التنقل وعرض لوحة المعلومات
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // الحصول على خدمة التنقل من مزود الخدمات المركزي
            var navigationService = AppServices.NavigationService;

            // الاشتراك في حدث طلب التنقل لتحديث المحتوى المعروض
            navigationService.NavigationRequested += OnNavigationRequested;

            // إنشاء نموذج العرض الرئيسي وربطه بالنافذة
            var viewModel = new MainViewModel(navigationService);
            this.DataContext = viewModel;

            // تحميل لوحة المعلومات كأول واجهة عند بدء التشغيل
            navigationService.NavigateTo("Dashboard");

            // تسجيل حدث Loaded للتحقق من تنبيهات المخزون المنخفض
            this.Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// معالج حدث التنقل
        /// يقوم بتحديث المحتوى المعروض في المنطقة الرئيسية للنافذة
        /// </summary>
        /// <param name="view">عنصر التحكم الجديد المراد عرضه</param>
        private void OnNavigationRequested(UserControl view)
        {
            MainContentArea.Content = view;
        }

        /// <summary>
        /// يتم استدعاؤه عند تحميل النافذة
        /// يتحقق من وجود منتجات منخفضة المخزون
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CheckForLowStockNotifications();
        }

        /// <summary>
        /// التحقق من المنتجات منخفضة المخزون وعرض تنبيه إذا وجدت
        /// يتطلب صلاحية عرض المخزون المنخفض
        /// </summary>
        private void CheckForLowStockNotifications()
        {
            // التحقق من الصلاحيات أولاً
            if (!PermissionsService.CanAccess("Inventory.LowStock.View")) return;

            // إنشاء نموذج عرض التنبيهات وتحميل البيانات
            var lowStockViewModel = new LowStockNotificationsViewModel();

            // عرض نافذة التنبيه فقط إذا وجدت عناصر منخفضة المخزون
            if (lowStockViewModel.HasLowStockItems)
            {
                // إنشاء واجهة التنبيه وربطها بنموذج العرض
                var notificationView = new LowStockNotificationsView();
                notificationView.DataContext = lowStockViewModel;

                // إنشاء نافذة منبثقة لعرض التنبيه
                var notificationWindow = new Window
                {
                    Title = "تنبيه انخفاض المخزون",
                    Content = notificationView,
                    Width = 800,
                    Height = 450,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    FlowDirection = FlowDirection.RightToLeft
                };
                notificationWindow.ShowDialog();
            }
        }

        /// <summary>
        /// يتم استدعاؤه عند إغلاق النافذة الرئيسية
        /// يقوم بإغلاق التطبيق بالكامل
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Application.Current.Shutdown();
        }
    }
}