namespace SharedKernel.Auditing;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    string? DeletedBy { get; set; }
    DateTime? DeletedAt { get; set; }
}
