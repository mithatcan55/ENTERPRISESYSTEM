namespace Infrastructure.Persistence.Entities;

public sealed class EntityChangeLog
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Action { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ChangedProperties { get; set; }
    public string? TableName { get; set; }
    public string? SchemaName { get; set; }
}
