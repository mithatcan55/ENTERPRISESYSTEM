using Application.Exceptions;
using Application.Pipeline;
using Identity.Application.Users.Commands;

namespace Identity.Infrastructure.Users.Commands;

public sealed class CreateUserCommandValidator : IRequestValidator<CreateUserCommand>
{
    public Task ValidateAsync(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Request.UserCode))
            errors["userCode"] = ["UserCode zorunludur."];

        if (string.IsNullOrWhiteSpace(request.Request.Username))
            errors["username"] = ["Username zorunludur."];

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

        return Task.CompletedTask;
    }
}
