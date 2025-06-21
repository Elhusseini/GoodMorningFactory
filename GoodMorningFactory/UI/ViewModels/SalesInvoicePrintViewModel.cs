using GoodMorningFactory.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GoodMorningFactory.UI.ViewModels
{
    // ViewModel خاص ببيانات الفاتورة للطباعة
    public class SalesInvoicePrintViewModel
    {
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public BitmapImage CompanyLogo { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue => TotalAmount - AmountPaid;
        public ICollection<SaleItemPrintViewModel> SaleItems { get; set; }
    }
}
