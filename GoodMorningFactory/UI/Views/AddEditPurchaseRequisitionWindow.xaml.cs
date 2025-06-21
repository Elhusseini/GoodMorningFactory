// GoodMorningFactory/UI/Views/AddEditPurchaseRequisitionWindow.xaml.cs
// *** الكود الكامل والمصحح ***
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditPurchaseRequisitionWindow : Window
    {
        /// <summary>
        /// المُنشئ الافتراضي للإضافة والتعديل.
        /// </summary>
        public AddEditPurchaseRequisitionWindow(int? requisitionId = null)
        {
            InitializeComponent();
            DataContext = new AddEditPurchaseRequisitionViewModel(requisitionId);
        }

        /// <summary>
        /// مُنشئ مخصص لـ MRP.
        /// </summary>
        public AddEditPurchaseRequisitionWindow(int productId, decimal quantity)
        {
            InitializeComponent();
            DataContext = new AddEditPurchaseRequisitionViewModel(productId, quantity);
        }
    }
}