// GoodMorningFactory/UI/Views/AddEditPurchaseOrderWindow.xaml.cs

// --- ملاحظة: هذا الملف هو الكود الخلفي للنافذة. ---
// --- وظيفته الأساسية هي إنشاء نسخة من الـ ViewModel وربطها بالواجهة (تعيين الـ DataContext). ---
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditPurchaseOrderWindow : Window
    {
        public AddEditPurchaseOrderWindow(int? poId = null, int? sourceRequisitionId = null)
        {
            InitializeComponent();

            // هذا السطر هو أهم سطر، فهو يخبر الواجهة بأن كل منطقها وبياناتها موجودة في AddEditPurchaseOrderViewModel
            DataContext = new AddEditPurchaseOrderViewModel(poId, sourceRequisitionId);
        }
    }
}