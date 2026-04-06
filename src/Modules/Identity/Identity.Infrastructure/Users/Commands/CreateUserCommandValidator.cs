using Application.Exceptions;
using Application.Pipeline;
using Identity.Application.Users.Commands;

namespace Identity.Infrastructure.Users.Commands;

public sealed class CreateUserCommandValidator : IRequestValidator<CreateUserCommand>
{
    public Task ValidateAsync(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Validator sadece request girisinin sekil kontrolune odaklanir.
        // Duplicate kontrolu veya password policy gibi daha agir is kurallari handler/policy katmaninda kalir.
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Request.UserCode))
            errors["userCode"] = ["UserCode zorunludur."];

        if (string.IsNullOrWhiteSpace(request.Request.Email))
            errors["email"] = ["Email zorunludur."];

        if (string.IsNullOrWhiteSpace(request.Request.Password))
            errors["password"] = ["Password zorunludur."];

        if (request.Request.CompanyId <= 0)
            errors["companyId"] = ["CompanyId pozitif bir deger olmalidir."];

        if (request.Request.NotifyAdminByMail && string.IsNullOrWhiteSpace(request.Request.AdminEmail))
            errors["adminEmail"] = ["NotifyAdminByMail=true ise adminEmail zorunludur."];

        if (errors.Count > 0)
            throw new ValidationAppException("CreateUserCommand dogrulamasi basarisiz.", errors);

        // Validator tamamlandiysa pipeline handler'a gecis izni verir.
        return Task.CompletedTask;
    }
}
