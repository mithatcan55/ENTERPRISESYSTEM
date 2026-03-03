namespace Infrastructure.Persistence.Entities;

public sealed class DatabaseQueryLog
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }
    public string? Operation { get; set; }
    public string? TableName { get; set; }
    public string? CommandText { get; set; }
    public string? Parameters { get; set; }
    public long DurationMs { get; set; }
    public int RowsAffected { get; set; }
    public bool IsError { get; set; }
    public string? ErrorMessage { get; set; }
}
