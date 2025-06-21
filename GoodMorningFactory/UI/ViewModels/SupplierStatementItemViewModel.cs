// UI/ViewModels/SupplierStatementItemViewModel.cs
// *** ملف جديد: ViewModel لعرض كشف حساب المورد ***
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class SupplierStatementItemViewModel
    {
        public DateTime Date { get; set; }
        public string TransactionType { get; set; } // نوع الحركة (فاتورة شراء، دفعة)
        public string ReferenceNumber { get; set; } // رقم الفاتورة أو سند الدفع
        public decimal Debit { get; set; } // المبالغ المستحقة (من الفواتير)
        public decimal Credit { get; set; } // المبالغ المسددة (من الدفعات)
        public decimal Balance { get; set; } // الرصيد المرحل
    }
}