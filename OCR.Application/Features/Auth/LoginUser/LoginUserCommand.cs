using MediatR;

namespace OCR.Application.Features.Auth.LoginUser
{
    public record LoginUserCommand
    (string Username,
        string Password) : IRequest<LoginUserResult>;

    public record LoginUserResult
    (
        string _jwtToken
        
    );
}
