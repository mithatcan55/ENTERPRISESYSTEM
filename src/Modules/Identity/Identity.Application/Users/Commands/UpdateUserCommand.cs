using Identity.Application.Contracts;

namespace Identity.Application.Users.Commands;

public sealed record UpdateUserCommand(int UserId, UpdateUserRequest Request);
