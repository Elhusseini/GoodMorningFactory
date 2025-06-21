// Core/Documents/PurchaseReportDocument.cs
// *** ملف جديد: كلاس مسؤول عن تصميم وتوليد تقرير المشتريات كملف PDF ***
using GoodMorningFactory.Data.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace GoodMorningFactory.Core.Documents
{
    public class PurchaseReportDocument : IDocument
    {
        private readonly List<Purchase> _purchases;
        private readonly CompanyInfo _companyInfo;
        private readonly string _period;

        public PurchaseReportDocument(List<Purchase> purchases, CompanyInfo companyInfo, string period)
        {
            _purchases = purchases;
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
                page.ContentFromRightToLeft();

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
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
                column.Item().Element(ComposeTable);
                var totalPrice = _purchases.Sum(x => x.TotalAmount);
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
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Text("رقم الفاتورة").Style(headerStyle);
                    header.Cell().Text("المورد").Style(headerStyle);
                    header.Cell().Text("تاريخ الشراء").Style(headerStyle);
                    header.Cell().Text("إجمالي المبلغ").Style(headerStyle);
                });

                foreach (var purchase in _purchases)
                {
                    table.Cell().Text(purchase.InvoiceNumber);
                    table.Cell().Text(purchase.Supplier?.Name ?? "غير محدد");
                    table.Cell().Text($"{purchase.PurchaseDate:d}");
                    table.Cell().Text($"{purchase.TotalAmount:C}");
                }
            });
        }
    }
}