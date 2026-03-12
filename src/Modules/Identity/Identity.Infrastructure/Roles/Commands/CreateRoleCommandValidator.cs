using Application.Exceptions;
using Application.Pipeline;
using Identity.Application.Roles.Commands;

namespace Identity.Infrastructure.Roles.Commands;

public sealed class CreateRoleCommandValidator : IRequestValidator<CreateRoleCommand>
{
    public Task ValidateAsync(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // Basit request kurallari validator'da tutulur; role benzersizligi gibi sorgu isteyen kurallar handler'dadir.
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Request.Code))
            errors["code"] = ["Code zorunludur."];

        if (string.IsNullOrWhiteSpace(request.Request.Name))
            errors["name"] = ["Name zorunludur."];

        if (errors.Count > 0)
            throw new ValidationAppException("CreateRoleCommand dogrulamasi basarisiz.", errors);

        return Task.CompletedTask;
    }
}
