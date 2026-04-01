using ClosedXML.Excel;
using Integrations.Application.Services;

namespace Integrations.Infrastructure.Services;

public sealed class DynamicExcelExporter : IExcelExporter
{
    public byte[] Export(IReadOnlyList<Dictionary<string, object?>> rows, string sheetName = "Data")
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        if (rows.Count == 0)
        {
            worksheet.Cell(1, 1).Value = "No data";
            return ToBytes(workbook);
        }

        var headers = rows[0].Keys.ToList();

        // Header row
        for (var col = 0; col < headers.Count; col++)
        {
            var cell = worksheet.Cell(1, col + 1);
            cell.Value = headers[col];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0c446d");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Data rows
        for (var row = 0; row < rows.Count; row++)
        {
            var dataRow = rows[row];
            for (var col = 0; col < headers.Count; col++)
            {
                var value = dataRow.GetValueOrDefault(headers[col]);
                SetCellValue(worksheet.Cell(row + 2, col + 1), value);
            }
        }

        worksheet.Columns().AdjustToContents(1, Math.Min(rows.Count + 1, 100));
        worksheet.RangeUsed()?.SetAutoFilter();

        return ToBytes(workbook);
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null: cell.Value = ""; break;
            case int i: cell.Value = i; break;
            case long l: cell.Value = l; break;
            case double d: cell.Value = d; break;
            case decimal dec: cell.Value = (double)dec; break;
            case float f: cell.Value = (double)f; break;
            case bool b: cell.Value = b; break;
            case DateTime dt:
                cell.Value = dt;
                cell.Style.NumberFormat.Format = "yyyy-MM-dd HH:mm";
                break;
            case DateTimeOffset dto:
                cell.Value = dto.LocalDateTime;
                cell.Style.NumberFormat.Format = "yyyy-MM-dd HH:mm";
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }

    private static byte[] ToBytes(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
