using GoodMorningFactory.UI.ViewModels;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class JournalVouchersView : UserControl
    {
        public JournalVouchersView()
        {
            InitializeComponent();
            // تعيين الـ ViewModel ليكون هو مصدر البيانات لهذه الواجهة.
            DataContext = new JournalVouchersViewModel();
        }
    }
}
