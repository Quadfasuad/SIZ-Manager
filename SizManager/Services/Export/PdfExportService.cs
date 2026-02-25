using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SizManager.Models;

namespace SizManager.Services.Export;

public class PdfExportService
{
    public void Export(Employee employee, ICollection<EmployeeSIZ> sizItems, string outputPath)
    {
        Document.Create(container =>
        {
            // Page 1: Лицевая сторона личной карточки
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Times New Roman"));

                page.Content().Column(col =>
                {
                    col.Spacing(3);

                    // Title
                    col.Item().AlignCenter().Text("ЛИЧНАЯ КАРТОЧКА УЧЕТА ВЫДАЧИ СИЗ")
                        .Bold().FontSize(12);
                    col.Item().AlignRight().Text("№ " + (employee.CardNumber ?? "___"))
                        .FontSize(10);
                    col.Item().AlignCenter().Text("Лицевая сторона личной карточки")
                        .FontSize(9).Italic();

                    col.Item().PaddingTop(8);

                    // Table 1: Employee data (matching DOCX layout)
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2.5f); // Label
                            c.RelativeColumn(3);    // Value
                            c.RelativeColumn(3);    // Right column (Пол/Рост/Размеры)
                        });

                        // Row 0: Фамилия | {LastName} | Пол {Gender}
                        table.Cell().Border(1).Padding(3).Text("Фамилия").FontSize(9);
                        table.Cell().Border(1).Padding(3).Text(employee.LastName).FontSize(9);
                        table.Cell().Border(1).Padding(3).Text("Пол " + (employee.Gender ?? "")).FontSize(9);

                        // Row 1: Имя | {FirstName} | Рост {Height}
                        table.Cell().Border(1).Padding(3).Text("Имя").FontSize(9);
                        table.Cell().Border(1).Padding(3).Text(employee.FirstName).FontSize(9);
                        table.Cell().Border(1).Padding(3).Text("Рост " + (employee.Height?.ToString() ?? "")).FontSize(9);

                        // Row 2: Отчество | {MiddleName} | Размеры
                        table.Cell().Border(1).Padding(3).Text("Отчество (при наличии)").FontSize(9);
                        table.Cell().Border(1).Padding(3).Text(employee.MiddleName ?? "").FontSize(9);
                        table.Cell().Border(1).Padding(3).Text(t =>
                        {
                            t.Span("Размер:").FontSize(8);
                            t.EmptyLine();
                            t.Span("одежды " + (employee.ClothingSize ?? "___")).FontSize(8);
                            t.EmptyLine();
                            t.Span("обуви " + (employee.ShoeSize ?? "___")).FontSize(8);
                            t.EmptyLine();
                            t.Span("головного убора " + (employee.HeadwearSize ?? "___")).FontSize(8);
                            t.EmptyLine();
                            t.Span("СИЗОД " + (employee.RespiratorsSize ?? "___")).FontSize(8);
                            t.EmptyLine();
                            t.Span("СИЗ рук " + (employee.GlovesSize ?? "___")).FontSize(8);
                        });

                        // Rows 3-7: single merged cells (ColumnSpan 3)
                        table.Cell().ColumnSpan(3).Border(1).Padding(3)
                            .Text("Табельный номер " + (employee.PersonnelNumber ?? "")).FontSize(9);
                        table.Cell().ColumnSpan(3).Border(1).Padding(3)
                            .Text("Структурное подразделение " + (employee.Department ?? "")).FontSize(9);
                        table.Cell().ColumnSpan(3).Border(1).Padding(3)
                            .Text("Профессия (должность) " + employee.ProfessionName).FontSize(9);
                        table.Cell().ColumnSpan(3).Border(1).Padding(3)
                            .Text("Дата поступления на работу " + (employee.HireDate?.ToString("dd.MM.yyyy") ?? "")).FontSize(9);
                        table.Cell().ColumnSpan(3).Border(1).Padding(3)
                            .Text("Дата изменения профессии (должности) или перевода в другое структурное подразделение " + (employee.ChangeDate?.ToString("dd.MM.yyyy") ?? "")).FontSize(9);
                    });

                    col.Item().PaddingTop(10);

                    // Table 2: SIZ items (matching DOCX columns exactly)
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);   // Наименование СИЗ
                            c.RelativeColumn(2);   // Пункт Норм
                            c.RelativeColumn(2.5f); // Единица измерения, периодичность выдачи
                            c.RelativeColumn(1.5f); // Количество на период
                        });

                        table.Header(header =>
                        {
                            header.Cell().Border(1).Padding(3).AlignCenter()
                                .Text("Наименование СИЗ").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(3).AlignCenter()
                                .Text("Пункт Норм").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(3).AlignCenter()
                                .Text("Единица измерения, периодичность выдачи").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(3).AlignCenter()
                                .Text("Количество на период").Bold().FontSize(8);
                        });

                        foreach (var siz in sizItems)
                        {
                            table.Cell().Border(1).Padding(3).Text(siz.Name).FontSize(8);
                            table.Cell().Border(1).Padding(3).Text(siz.Type).FontSize(8);
                            table.Cell().Border(1).Padding(3).Text(siz.Norm).FontSize(8);
                            table.Cell().Border(1).Padding(3).Text("").FontSize(8);
                        }
                    });

                    col.Item().PaddingTop(20);
                    col.Item().Text("Ответственное лицо за ведение карточек учета выдачи СИЗ").FontSize(9);
                    col.Item().PaddingTop(15);
                    col.Item().Text("_______________________     _______________________________").FontSize(9);
                    col.Item().Text("         (подпись)                      (фамилия, инициалы)").FontSize(8);
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.CurrentPageNumber().FontSize(8);
                    t.Span(" / ").FontSize(8);
                    t.TotalPages().FontSize(8);
                });
            });

            // Page 2: Оборотная сторона личной карточки
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Times New Roman"));

                page.Header().AlignCenter()
                    .Text("Оборотная сторона личной карточки").Bold().FontSize(11);

                page.Content().PaddingVertical(5).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);    // 1: Наименование СИЗ
                            c.RelativeColumn(3);    // 2: Модель, марка...
                            c.RelativeColumn(1.2f); // 3: Дата (выдано)
                            c.RelativeColumn(1);    // 4: Кол-во (выдано)
                            c.RelativeColumn(1.2f); // 5: Лично/дозатор
                            c.RelativeColumn(1.5f); // 6: Подпись получившего
                            c.RelativeColumn(1.2f); // 7: Дата (возврат)
                            c.RelativeColumn(1);    // 8: Кол-во (возврат)
                            c.RelativeColumn(1.5f); // 9: Подпись сдавшего
                            c.RelativeColumn(1.5f); // 10: Акт списания
                        });

                        table.Header(header =>
                        {
                            header.Cell().RowSpan(2).Border(1).Padding(2).AlignCenter().AlignMiddle()
                                .Text("Наименование СИЗ").Bold().FontSize(7);
                            header.Cell().RowSpan(2).Border(1).Padding(2).AlignCenter().AlignMiddle()
                                .Text("Модель, марка, артикул, класс защиты СИЗ, дерматологических СИЗ").Bold().FontSize(7);
                            header.Cell().ColumnSpan(4).Border(1).Padding(2).AlignCenter()
                                .Text("Выдано").Bold().FontSize(7);
                            header.Cell().ColumnSpan(4).Border(1).Padding(2).AlignCenter()
                                .Text("Возвращено**").Bold().FontSize(7);

                            header.Cell().Border(1).Padding(1).AlignCenter().Text("дата").FontSize(6);
                            header.Cell().Border(1).Padding(1).AlignCenter().Text("количество").FontSize(6);
                            header.Cell().Border(1).Padding(1).AlignCenter().Text("Лично/ дозатор*").FontSize(6);
                            header.Cell().Border(1).Padding(1).AlignCenter().Text("подпись получившего СИЗ").FontSize(6);
                            header.Cell().Border(1).Padding(1).AlignCenter().Text("дата").FontSize(6);
                            header.Cell().Border(1).Padding(1).AlignCenter().Text("количество").FontSize(6);
                            header.Cell().Border(1).Padding(1).AlignCenter().Text("Подпись сдавшего СИЗ").FontSize(6);
                            header.Cell().Border(1).Padding(1).AlignCenter().Text("Акт списания (дата, номер)").FontSize(6);
                        });

                        var sizList = sizItems.ToList();
                        int totalRows = Math.Max(sizList.Count, 12);
                        for (int i = 0; i < totalRows; i++)
                        {
                            string sizName = i < sizList.Count ? sizList[i].Name : "";
                            table.Cell().Border(1).Padding(2).Text(sizName).FontSize(7);
                            for (int c = 1; c < 10; c++)
                                table.Cell().Border(1).Padding(2).MinHeight(20).Text("").FontSize(7);
                        }
                    });

                    col.Item().PaddingTop(5);
                    col.Item().Text("* — информация указывается только для дерматологических СИЗ").FontSize(6);
                    col.Item().Text("** — информация указывается для всех СИЗ, кроме дерматологических СИЗ и СИЗ однократного применения").FontSize(6);
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.CurrentPageNumber().FontSize(8);
                    t.Span(" / ").FontSize(8);
                    t.TotalPages().FontSize(8);
                });
            });
        }).GeneratePdf(outputPath);
    }
}
