﻿// UI/ViewModels/LedgerEntryViewModel.cs
// *** ملف جديد: ViewModel خاص بعرض بيانات دفتر الأستاذ ***
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class LedgerEntryViewModel
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
    }
}