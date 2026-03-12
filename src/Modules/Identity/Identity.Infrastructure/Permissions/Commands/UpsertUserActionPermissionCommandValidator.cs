using Application.Exceptions;
using Application.Pipeline;
using Identity.Application.Permissions.Commands;

namespace Identity.Infrastructure.Permissions.Commands;

public sealed class UpsertUserActionPermissionCommandValidator : IRequestValidator<UpsertUserActionPermissionCommand>
{
    public Task ValidateAsync(UpsertUserActionPermissionCommand request, CancellationToken cancellationToken)
    {
        // Bu validator'in kritik noktasi "hangi sayfayi hedefledigin acik olmali" kuralidir.
        // Bu nedenle SubModulePageId veya TransactionCode zorunludur.
        var errors = new Dictionary<string, string[]>();

        if (request.Request.UserId <= 0)
            errors["userId"] = ["Gecerli bir userId zorunludur."];

        if (string.IsNullOrWhiteSpace(request.Request.ActionCode))
            errors["actionCode"] = ["ActionCode zorunludur."];

        if (!request.Request.SubModulePageId.HasValue && string.IsNullOrWhiteSpace(request.Request.TransactionCode))
        {
            errors["subModulePageId"] = ["SubModulePageId veya TransactionCode zorunludur."];
            errors["transactionCode"] = ["SubModulePageId veya TransactionCode zorunludur."];
        }

        if (errors.Count > 0)
            throw new ValidationAppException("UpsertUserActionPermissionCommand dogrulamasi basarisiz.", errors);

        return Task.CompletedTask;
    }
}
