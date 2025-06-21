// GoodMorningFactory/UI/Views/AccountingPeriodsView.xaml.cs
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    /// <summary>
    /// تم نقل كل المنطق البرمجي من هنا إلى AccountingPeriodsViewModel.
    /// أصبح هذا الملف مسؤولاً فقط عن تهيئة الواجهة.
    /// </summary>
    public partial class AccountingPeriodsView : UserControl
    {
        public AccountingPeriodsView()
        {
            InitializeComponent();
            // الـ ViewModel يتم تعيينه الآن في ملف XAML
        }
    }
}