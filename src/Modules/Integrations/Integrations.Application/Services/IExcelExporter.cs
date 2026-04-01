namespace Integrations.Application.Services;

public interface IExcelExporter
{
    byte[] Export(IReadOnlyList<Dictionary<string, object?>> rows, string sheetName = "Data");
}
