// Core/Documents/InventoryReportDocument.cs
// *** ملف جديد: كلاس مسؤول عن تصميم وتوليد تقرير المخزون كملف PDF ***
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;

namespace GoodMorningFactory.Core.Documents
{
    public class InventoryReportDocument : IDocument
    {
        private readonly List<InventoryViewModel> _inventory;
        private readonly CompanyInfo _companyInfo;

        public InventoryReportDocument(List<InventoryViewModel> inventory, CompanyInfo companyInfo)
        {
            _inventory = inventory;
            _companyInfo = companyInfo;
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
                    column.Item().Text("تقرير حالة المخزون الحالية");
                    column.Item().Text($"تاريخ الطباعة: {System.DateTime.Now:d}");
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Item().Element(ComposeTable);
            });
        }

        void ComposeTable(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1.5f);
                });

                table.Header(header =>
                {
                    header.Cell().Text("كود المنتج").Style(headerStyle);
                    header.Cell().Text("اسم المنتج").Style(headerStyle);
                    header.Cell().Text("التصنيف").Style(headerStyle);
                    header.Cell().Text("الكمية الحالية").Style(headerStyle);
                });

                foreach (var item in _inventory)
                {
                    table.Cell().Text(item.ProductCode);
                    table.Cell().Text(item.ProductName);
                    table.Cell().Text(item.CategoryName);
                    table.Cell().Text(item.Quantity.ToString()).SemiBold();
                }
            });
        }
    }
}