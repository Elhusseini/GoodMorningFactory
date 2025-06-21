// UI/Views/PrintPreviewWindow.xaml.cs
// *** تحديث: تم تغيير نوع المستند الذي تقبله النافذة ***
using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;

namespace GoodMorningFactory.UI.Views
{
    public partial class PrintPreviewWindow : Window
    {
        private readonly string _tempXpsFilePath;

        // *** بداية التصحيح ***
        // تغيير نوع المستند إلى FixedDocumentSequence ليتوافق مع DocumentViewer
        // إضافة مسار الملف المؤقت (اختياري)
        public PrintPreviewWindow(FixedDocumentSequence document, string tempXpsFilePath = null)
        // *** نهاية التصحيح ***
        {
            InitializeComponent();
            DocumentViewer.Document = document;
            _tempXpsFilePath = tempXpsFilePath;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // حذف الملف المؤقت بعد إغلاق النافذة
            if (!string.IsNullOrEmpty(_tempXpsFilePath) && File.Exists(_tempXpsFilePath))
            {
                try { File.Delete(_tempXpsFilePath); } catch { /* تجاهل أي خطأ */ }
            }
        }
    }
}