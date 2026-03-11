namespace Infrastructure.Persistence.Auditing;

/// <summary>
/// Veriyi değiştiren mevcut aktörün kimlik bilgisini sağlar.
/// Bu soyutlama sayesinde DbContext katmanı doğrudan HttpContext'e bağımlı olmadan
/// CreatedBy/ModifiedBy/DeletedBy alanlarını güvenilir şekilde doldurabilir.
/// </summary>
public interface IAuditActorAccessor
{
    string GetActorId();
}
