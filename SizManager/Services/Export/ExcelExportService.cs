using ClosedXML.Excel;
using SizManager.Models;

namespace SizManager.Services.Export;

public class ExcelExportService
{
    public void Export(Employee employee, ICollection<EmployeeSIZ> sizItems, string outputPath)
    {
        using var workbook = new XLWorkbook();

        // Sheet 1: Лицевая сторона (Table 1 + Table 2 matching DOCX)
        var frontSheet = workbook.Worksheets.Add("Лицевая сторона");
        BuildFrontSide(frontSheet, employee, sizItems);

        // Sheet 2: Оборотная сторона (Table 3 matching DOCX)
        var backSheet = workbook.Worksheets.Add("Оборотная сторона");
        BuildBackSide(backSheet, sizItems);

        workbook.SaveAs(outputPath);
    }

    private static void BuildFrontSide(IXLWorksheet sheet, Employee employee, ICollection<EmployeeSIZ> sizItems)
    {
        // Title
        sheet.Cell(1, 1).Value = "ЛИЧНАЯ КАРТОЧКА УЧЕТА ВЫДАЧИ СИЗ";
        sheet.Range(1, 1, 1, 3).Merge();
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        sheet.Cell(1, 4).Value = "№ " + (employee.CardNumber ?? "___");
        sheet.Cell(1, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        sheet.Cell(2, 1).Value = "Лицевая сторона личной карточки";
        sheet.Range(2, 1, 2, 4).Merge();
        sheet.Cell(2, 1).Style.Font.Italic = true;
        sheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Table 1: Employee data (rows 4-11, matching DOCX 3-column layout)
        int r = 4;

        // Row 0: Фамилия | LastName | Пол Gender
        SetCell(sheet, r, 1, "Фамилия", bold: true);
        SetCell(sheet, r, 2, employee.LastName);
        SetCell(sheet, r, 3, "Пол " + (employee.Gender ?? ""));
        AddBorders(sheet, r, 1, r, 3);
        r++;

        // Row 1: Имя | FirstName | Рост Height
        SetCell(sheet, r, 1, "Имя", bold: true);
        SetCell(sheet, r, 2, employee.FirstName);
        SetCell(sheet, r, 3, "Рост " + (employee.Height?.ToString() ?? ""));
        AddBorders(sheet, r, 1, r, 3);
        r++;

        // Row 2: Отчество | MiddleName | Размеры
        SetCell(sheet, r, 1, "Отчество (при наличии)", bold: true);
        SetCell(sheet, r, 2, employee.MiddleName ?? "");
        var sizes = "Размер:\n" +
                    "одежды " + (employee.ClothingSize ?? "___") + "\n" +
                    "обуви " + (employee.ShoeSize ?? "___") + "\n" +
                    "головного убора " + (employee.HeadwearSize ?? "___") + "\n" +
                    "СИЗОД " + (employee.RespiratorsSize ?? "___") + "\n" +
                    "СИЗ рук " + (employee.GlovesSize ?? "___");
        SetCell(sheet, r, 3, sizes);
        sheet.Cell(r, 3).Style.Alignment.WrapText = true;
        AddBorders(sheet, r, 1, r, 3);
        r++;

        // Rows 3-7: merged single-column rows
        SetMergedRow(sheet, r, 1, 3, "Табельный номер " + (employee.PersonnelNumber ?? ""));
        r++;
        SetMergedRow(sheet, r, 1, 3, "Структурное подразделение " + (employee.Department ?? ""));
        r++;
        SetMergedRow(sheet, r, 1, 3, "Профессия (должность) " + employee.ProfessionName);
        r++;
        SetMergedRow(sheet, r, 1, 3, "Дата поступления на работу " + (employee.HireDate?.ToString("dd.MM.yyyy") ?? ""));
        r++;
        SetMergedRow(sheet, r, 1, 3, "Дата изменения профессии (должности) или перевода в другое структурное подразделение " + (employee.ChangeDate?.ToString("dd.MM.yyyy") ?? ""));
        r++;

        // Table 2: SIZ items (matching DOCX columns)
        r += 2;

        // Header
        SetCell(sheet, r, 1, "Наименование СИЗ", bold: true);
        SetCell(sheet, r, 2, "Пункт Норм", bold: true);
        SetCell(sheet, r, 3, "Единица измерения, периодичность выдачи", bold: true);
        SetCell(sheet, r, 4, "Количество на период", bold: true);
        var headerRange = sheet.Range(r, 1, r, 4);
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.WrapText = true;
        AddBorders(sheet, r, 1, r, 4);
        r++;

        // Data rows
        foreach (var siz in sizItems)
        {
            sheet.Cell(r, 1).Value = siz.Name;
            sheet.Cell(r, 2).Value = siz.Type;
            sheet.Cell(r, 3).Value = siz.Norm;
            sheet.Cell(r, 4).Value = "";
            AddBorders(sheet, r, 1, r, 4);
            r++;
        }

        // Signature
        r += 2;
        sheet.Cell(r, 1).Value = "Ответственное лицо за ведение карточек учета выдачи СИЗ";
        sheet.Range(r, 1, r, 3).Merge();
        r += 2;
        sheet.Cell(r, 1).Value = "_______________________";
        sheet.Cell(r, 2).Value = "_______________________________";
        r++;
        sheet.Cell(r, 1).Value = "(подпись)";
        sheet.Cell(r, 2).Value = "(фамилия, инициалы)";

        // Column widths
        sheet.Column(1).Width = 30;
        sheet.Column(2).Width = 30;
        sheet.Column(3).Width = 35;
        sheet.Column(4).Width = 20;
    }

    private static void BuildBackSide(IXLWorksheet sheet, ICollection<EmployeeSIZ> sizItems)
    {
        // Header row 1: merged groups
        sheet.Cell(1, 1).Value = "Наименование СИЗ";
        sheet.Range(1, 1, 2, 1).Merge();
        sheet.Cell(1, 2).Value = "Модель, марка, артикул, класс защиты СИЗ, дерматологических СИЗ";
        sheet.Range(1, 2, 2, 2).Merge();
        sheet.Cell(1, 3).Value = "Выдано";
        sheet.Range(1, 3, 1, 6).Merge();
        sheet.Cell(1, 7).Value = "Возвращено**";
        sheet.Range(1, 7, 1, 10).Merge();

        // Header row 2: sub-columns
        sheet.Cell(2, 3).Value = "дата";
        sheet.Cell(2, 4).Value = "количество";
        sheet.Cell(2, 5).Value = "Лично/дозатор*";
        sheet.Cell(2, 6).Value = "подпись получившего СИЗ";
        sheet.Cell(2, 7).Value = "дата";
        sheet.Cell(2, 8).Value = "количество";
        sheet.Cell(2, 9).Value = "Подпись сдавшего СИЗ";
        sheet.Cell(2, 10).Value = "Акт списания (дата, номер)";

        // Style headers
        var headerRange = sheet.Range(1, 1, 2, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        headerRange.Style.Alignment.WrapText = true;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Data rows: pre-fill SIZ names
        var sizList = sizItems.ToList();
        int totalRows = Math.Max(sizList.Count, 12);
        for (int i = 0; i < totalRows; i++)
        {
            int r = 3 + i;
            if (i < sizList.Count)
                sheet.Cell(r, 1).Value = sizList[i].Name;
            sheet.Range(r, 1, r, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.Range(r, 1, r, 10).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        // Footnotes
        int footRow = 3 + totalRows + 1;
        sheet.Cell(footRow, 1).Value = "* — информация указывается только для дерматологических СИЗ";
        sheet.Cell(footRow + 1, 1).Value = "** — информация указывается для всех СИЗ, кроме дерматологических СИЗ и СИЗ однократного применения";

        // Column widths
        sheet.Column(1).Width = 25;
        sheet.Column(2).Width = 25;
        for (int c = 3; c <= 10; c++)
            sheet.Column(c).Width = 14;
    }

    private static void SetCell(IXLWorksheet sheet, int row, int col, string value, bool bold = false)
    {
        sheet.Cell(row, col).Value = value;
        if (bold)
            sheet.Cell(row, col).Style.Font.Bold = true;
    }

    private static void SetMergedRow(IXLWorksheet sheet, int row, int fromCol, int toCol, string value)
    {
        sheet.Cell(row, fromCol).Value = value;
        sheet.Range(row, fromCol, row, toCol).Merge();
        sheet.Range(row, fromCol, row, toCol).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    private static void AddBorders(IXLWorksheet sheet, int r1, int c1, int r2, int c2)
    {
        var range = sheet.Range(r1, c1, r2, c2);
        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
    }
}
