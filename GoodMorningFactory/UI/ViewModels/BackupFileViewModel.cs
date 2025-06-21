// UI/ViewModels/BackupFileViewModel.cs
// *** ملف جديد: ViewModel خاص بعرض بيانات ملفات النسخ الاحتياطي ***
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class BackupFileViewModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime CreationDate { get; set; }
        public string FileSize { get; set; }
    }
}