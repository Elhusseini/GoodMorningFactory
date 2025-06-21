// Core/Documents/SalesReportDocument.cs
// *** ملف جديد: كلاس مسؤول عن تصميم وتوليد تقرير المبيعات كملف PDF ***
using GoodMorningFactory.Data.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace GoodMorningFactory.Core.Documents
{
    public class SalesReportDocument : IDocument
    {
        private readonly List<Sale> _sales;
        private readonly CompanyInfo _companyInfo;
        private readonly string _period;

        public SalesReportDocument(List<Sale> sales, CompanyInfo companyInfo, string period)
        {
            _sales = sales;
            _companyInfo = companyInfo;
            _period = period;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Calibri));

                // تطبيق الاتجاه من اليمين لليسار
                page.ContentFromRightToLeft();

                // 1. ترويسة التقرير
                page.Header().Element(ComposeHeader);

                // 2. محتوى التقرير (الجدول)
                page.Content().Element(ComposeContent);

                // 3. تذييل الصفحة
                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        }

        void ComposeHeader(IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(_companyInfo?.CompanyName ?? "مصنع صباح الخير", titleStyle);
                    column.Item().Text(_period);
                    column.Item().Text($"تاريخ الطباعة: {System.DateTime.Now:d}");
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                // جدول البيانات
                column.Item().Element(ComposeTable);

                // الإجمالي
                var totalPrice = _sales.Sum(x => x.TotalAmount);
                column.Item().AlignRight().PaddingTop(5).Text($"الإجمالي: {totalPrice:C}", TextStyle.Default.SemiBold());
            });
        }

        void ComposeTable(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Text("رقم الفاتورة").Style(headerStyle);
                    header.Cell().Text("تاريخ البيع").Style(headerStyle);
                    header.Cell().Text("المبلغ المدفوع").Style(headerStyle);
                    header.Cell().Text("إجمالي المبلغ").Style(headerStyle);
                });

                foreach (var sale in _sales)
                {
                    table.Cell().Text(sale.InvoiceNumber);
                    table.Cell().Text($"{sale.SaleDate:g}");
                    table.Cell().Text($"{sale.AmountPaid:C}");
                    table.Cell().Text($"{sale.TotalAmount:C}");
                }
            });
        }
    }
}