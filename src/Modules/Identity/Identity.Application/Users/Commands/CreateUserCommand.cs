using Identity.Application.Contracts;

namespace Identity.Application.Users.Commands;

public sealed record CreateUserCommand(CreateUserRequest Request);
