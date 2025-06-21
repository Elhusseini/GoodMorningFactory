// UI/ViewModels/CustomerStatementItemViewModel.cs
// *** ملف جديد: ViewModel لعرض كشف حساب عميل تفصيلي ***
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    public class CustomerStatementItemViewModel
    {
        public DateTime Date { get; set; }
        public string TransactionType { get; set; } // نوع الحركة (فاتورة، دفعة، مرتجع)
        public string ReferenceNumber { get; set; } // رقم الفاتورة أو المرتجع
        public decimal Debit { get; set; } // المبلغ المستحق (من الفواتير)
        public decimal Credit { get; set; } // المبلغ المسدد (من الدفعات والمرتجعات)
        public decimal Balance { get; set; } // الرصيد المرحل
    }
}