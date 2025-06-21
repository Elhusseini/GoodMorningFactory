// UI/Views/PrintPreviewWindow.xaml.cs
// *** تحديث: تم تغيير نوع المستند الذي تقبله النافذة ***
using System.Windows;
using System.Windows.Documents;

namespace GoodMorningFactory.UI.Views
{
    public partial class PrintPreviewWindow : Window
    {
        // *** بداية التصحيح ***
        // تغيير نوع المستند إلى FixedDocumentSequence ليتوافق مع DocumentViewer
        public PrintPreviewWindow(FixedDocumentSequence document)
        // *** نهاية التصحيح ***
        {
            InitializeComponent();
            DocumentViewer.Document = document;
        }
    }
}