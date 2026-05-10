using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SizManager.ViewModels;

namespace SizManager.Services.Export;

public class DermatologicalPdfExportService
{
    public void Export(IEnumerable<DermatologicalRequirementRow> rows, string outputPath)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.2f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Times New Roman"));

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("Единые типовые нормы выдачи дерматологических средств индивидуальной защиты и смывающих средств")
                        .Bold().FontSize(12);
                    column.Item().PaddingTop(4).AlignCenter().Text("Сформировано по выбранным видам работ")
                        .FontSize(9);
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(1.2f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4).AlignCenter()
                            .Text("Необходимое средство").Bold();
                        header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4).AlignCenter()
                            .Text("Норма выдачи на 1 месяц").Bold();
                    });

                    foreach (var row in rows)
                    {
                        if (row.IsHeader)
                        {
                            table.Cell().ColumnSpan(2).Border(1).Background(Colors.Blue.Lighten5).Padding(4)
                                .Text(row.ProductType).Bold();
                            continue;
                        }

                        table.Cell().Border(1).Padding(4).Text(row.ProductType);
                        table.Cell().Border(1).Padding(4).AlignCenter().Text(row.Norm);
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber().FontSize(8);
                    text.Span(" / ").FontSize(8);
                    text.TotalPages().FontSize(8);
                });
            });
        }).GeneratePdf(outputPath);
    }
}
