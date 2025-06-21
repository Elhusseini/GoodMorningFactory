using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// واجهة إدارة المنتجات
    /// تعرض قائمة المنتجات وتوفر وظائف إدارة المنتجات والتصنيفات ووحدات القياس وقوائم الأسعار
    /// </summary>
    public partial class ProductsView : UserControl
    {
        /// <summary>
        /// منشئ واجهة المنتجات
        /// يقوم بتهيئة المكونات وربط نموذج العرض
        /// </summary>
        public ProductsView()
        {
            InitializeComponent();
            // ربط نموذج عرض المنتجات
            // سيقوم تلقائياً بتحميل البيانات وإعداد الأوامر
            DataContext = new ProductsViewViewModel();
        }

        /// <summary>
        /// معالج النقر على زر إدارة التصنيفات
        /// ينتقل إلى واجهة إدارة تصنيفات المنتجات
        /// </summary>
        private void ManageCategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.NavigationService.NavigateTo("Categories");
        }

        /// <summary>
        /// معالج النقر على زر إدارة وحدات القياس
        /// ينتقل إلى واجهة إدارة وحدات القياس
        /// </summary>
        private void ManageUomButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.NavigationService.NavigateTo("UnitsOfMeasure");
        }

        /// <summary>
        /// معالج النقر على زر إدارة قوائم الأسعار
        /// ينتقل إلى واجهة إدارة قوائم الأسعار
        /// </summary>
        private void ManagePriceListsButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.NavigationService.NavigateTo("PriceLists");
        }
    }
}