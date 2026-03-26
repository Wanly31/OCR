using MediatR;

namespace OCR.Application.Features.Auth.RegisterUser
{
    public record RegisterUserCommand
    (
        string Username,
        string Password
    ) : IRequest<RegisterUserResult>;

    public record RegisterUserResult
    (
        string _jwtToken
    );
}
