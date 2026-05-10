using ClosedXML.Excel;
using SizManager.ViewModels;

namespace SizManager.Services.Export;

public class DermatologicalExcelExportService
{
    public void Export(IEnumerable<DermatologicalRequirementRow> rows, string outputPath)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Дерматологические СИЗ");

        sheet.Cell(1, 1).Value = "Единые типовые нормы выдачи дерматологических средств индивидуальной защиты и смывающих средств";
        sheet.Range(1, 1, 1, 2).Merge();
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 13;
        sheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Cell(1, 1).Style.Alignment.WrapText = true;

        var headers = new[]
        {
            "Необходимое средство",
            "Норма выдачи на 1 месяц"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cell(3, i + 1).Value = headers[i];
        }

        var headerRange = sheet.Range(3, 1, 3, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        headerRange.Style.Alignment.WrapText = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8EEF7");
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        var currentRow = 4;
        foreach (var row in rows)
        {
            if (row.IsHeader)
            {
                sheet.Cell(currentRow, 1).Value = row.ProductType;
                sheet.Range(currentRow, 1, currentRow, headers.Length).Merge();
                var groupRange = sheet.Range(currentRow, 1, currentRow, headers.Length);
                groupRange.Style.Font.Bold = true;
                groupRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#DDEBF7");
                groupRange.Style.Alignment.WrapText = true;
                groupRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            else
            {
                sheet.Cell(currentRow, 1).Value = row.ProductType;
                sheet.Cell(currentRow, 2).Value = row.Norm;
                var range = sheet.Range(currentRow, 1, currentRow, headers.Length);
                range.Style.Alignment.WrapText = true;
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }
            currentRow++;
        }

        sheet.Column(1).Width = 45;
        sheet.Column(2).Width = 22;

        sheet.Rows(3, Math.Max(3, currentRow - 1)).AdjustToContents();
        workbook.SaveAs(outputPath);
    }
}
