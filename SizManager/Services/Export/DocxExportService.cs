using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SizManager.Helpers;
using SizManager.Models;

namespace SizManager.Services.Export;

public class DocxExportService
{
    public void Export(Employee employee, ICollection<EmployeeSIZ> sizItems, string outputPath)
    {
        var templatePath = AppPaths.TemplatePath;
        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Шаблон DOCX не найден", templatePath);

        // Copy template to output
        File.Copy(templatePath, outputPath, overwrite: true);

        using var doc = WordprocessingDocument.Open(outputPath, true);
        var body = doc.MainDocumentPart!.Document.Body!;

        // Merge split placeholder runs (Word splits {Placeholder} across multiple runs)
        OpenXmlHelper.MergePlaceholderRuns(body);

        // Replace all placeholders in Table 1 and free-standing text
        var replacements = new Dictionary<string, string>
        {
            ["{CardNumber}"] = employee.CardNumber ?? "",
            ["{LastName}"] = employee.LastName,
            ["{FirstName}"] = employee.FirstName,
            ["{MiddleName}"] = employee.MiddleName ?? "",
            ["{Gender}"] = employee.Gender ?? "",
            ["{Height}"] = employee.Height?.ToString() ?? "",
            ["{ClothingSize}"] = employee.ClothingSize ?? "",
            ["{ShoeSize}"] = employee.ShoeSize ?? "",
            ["{HeadwearSize}"] = employee.HeadwearSize ?? "",
            ["{RespiratorsSize}"] = employee.RespiratorsSize ?? "",
            ["{GlovesSize}"] = employee.GlovesSize ?? "",
            ["{PersonnelNumber}"] = employee.PersonnelNumber ?? "",
            ["{Department}"] = employee.Department ?? "",
            ["{Profession}"] = employee.ProfessionName,
            ["{HireDate}"] = employee.HireDate?.ToString("dd.MM.yyyy") ?? "",
            ["{ChangeDate}"] = employee.ChangeDate?.ToString("dd.MM.yyyy") ?? "",
        };

        OpenXmlHelper.ReplacePlaceholders(body, replacements);

        // Fill SIZ table (Table 2 — front side)
        FillSizTable(body, sizItems);

        // Fill back side table (Table 3 — "Оборотная сторона") — pre-fill SIZ names
        FillBackSideTable(body, sizItems);

        doc.MainDocumentPart.Document.Save();
    }

    /// <summary>
    /// Fill Table 3 ("Оборотная сторона личной карточки") — pre-fill SIZ names in column 0.
    /// Other columns are left empty for hand-filling by the employee.
    /// Table 3 structure: Row 0 = merged header, Row 1 = sub-headers, Row 2 = column numbers, Rows 3+ = data.
    /// </summary>
    private void FillBackSideTable(Body body, ICollection<EmployeeSIZ> sizItems)
    {
        var tables = body.Descendants<Table>().ToList();
        if (tables.Count < 3) return;

        var backTable = tables[2];
        var rows = backTable.Elements<TableRow>().ToList();

        const int dataStartRow = 3; // Rows 0-2 are headers
        var sizList = sizItems.ToList();
        int availableDataRows = rows.Count - dataStartRow;

        // If more SIZ items than available rows, clone the last data row
        if (sizList.Count > availableDataRows && availableDataRows > 0)
        {
            var templateRow = rows[rows.Count - 1];
            for (int i = 0; i < sizList.Count - availableDataRows; i++)
            {
                var newRow = (TableRow)templateRow.CloneNode(true);
                foreach (var cell in newRow.Elements<TableCell>())
                    OpenXmlHelper.SetCellText(cell, "");
                backTable.AppendChild(newRow);
            }
            rows = backTable.Elements<TableRow>().ToList();
        }

        // Fill column 0 (Наименование СИЗ) with SIZ names
        for (int i = 0; i < sizList.Count && (dataStartRow + i) < rows.Count; i++)
        {
            var row = rows[dataStartRow + i];
            var cells = row.Elements<TableCell>().ToList();
            if (cells.Count > 0)
            {
                OpenXmlHelper.SetCellText(cells[0], sizList[i].Name);
            }
        }
    }

    private void FillSizTable(Body body, ICollection<EmployeeSIZ> sizItems)
    {
        var tables = body.Descendants<Table>().ToList();
        if (tables.Count < 2) return;

        // Table 2 structure (from template):
        //   Row 0: Header (Наименование СИЗ | Пункт норм | Единица/кол-во | Примечание)
        //   Row 1: {SIZ_ROW_START} marker row (4 cells)
        //   Rows 2-10: Empty template rows (4 cells each)
        //   Row 11: Last empty row
        var sizTable = tables[1];
        var rows = sizTable.Elements<TableRow>().ToList();

        // Find marker row with {SIZ_ROW_START}
        int markerIdx = -1;
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].InnerText.Contains("{SIZ_ROW_START}"))
            {
                markerIdx = i;
                break;
            }
        }

        if (markerIdx < 0) return;

        var markerRow = rows[markerIdx];

        // Insert data rows before the marker row
        foreach (var siz in sizItems)
        {
            var newRow = (TableRow)markerRow.CloneNode(true);
            var cells = newRow.Elements<TableCell>().ToList();

            if (cells.Count >= 4)
            {
                // Col 0: Наименование СИЗ
                OpenXmlHelper.SetCellText(cells[0], siz.Name);
                // Col 1: Пункт типовых норм (тип СИЗ)
                OpenXmlHelper.SetCellText(cells[1], siz.Type);
                // Col 2: Единица измерения, количество (норма выдачи)
                OpenXmlHelper.SetCellText(cells[2], siz.Norm);
                // Col 3: Примечание
                OpenXmlHelper.SetCellText(cells[3], "");
            }

            sizTable.InsertBefore(newRow, markerRow);
        }

        // Remove the marker row itself
        markerRow.Remove();

        // Remove excess empty template rows (keep table structure clean)
        // After inserting data and removing marker, re-read rows
        var updatedRows = sizTable.Elements<TableRow>().ToList();
        // Row 0 = header, Rows 1..N = our data, then empty rows follow
        int dataRowsEnd = 1 + sizItems.Count; // 1 for header + data count

        // Remove empty rows after data (they were blank template placeholders)
        for (int i = updatedRows.Count - 1; i >= dataRowsEnd; i--)
        {
            var rowText = updatedRows[i].InnerText.Trim();
            if (string.IsNullOrEmpty(rowText))
            {
                updatedRows[i].Remove();
            }
        }
    }
}
