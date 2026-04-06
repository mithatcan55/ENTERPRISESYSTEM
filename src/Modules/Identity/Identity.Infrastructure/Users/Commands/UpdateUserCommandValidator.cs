using Application.Exceptions;
using Application.Pipeline;
using Identity.Application.Users.Commands;

namespace Identity.Infrastructure.Users.Commands;

public sealed class UpdateUserCommandValidator : IRequestValidator<UpdateUserCommand>
{
    public Task ValidateAsync(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.UserId <= 0)
            errors["userId"] = ["Gecerli bir userId zorunludur."];

        if (string.IsNullOrWhiteSpace(request.Request.Email))
            errors["email"] = ["Email zorunludur."];

        if (errors.Count > 0)
            throw new ValidationAppException("UpdateUserCommand dogrulamasi basarisiz.", errors);

        return Task.CompletedTask;
    }
}
