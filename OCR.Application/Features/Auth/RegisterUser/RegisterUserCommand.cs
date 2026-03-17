

using MediatR;

namespace OCR.Application.Features.Auth.RegisterUser
{
    public record RegisterUserCommand
    (
        string Username,
        string Password,
        string Roles
    ) : IRequest<RegisterUserResult>;

    public record RegisterUserResult
    (
        string Username
    );
}
