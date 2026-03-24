using MediatR;
using Microsoft.AspNetCore.Identity;
using OCR.Application.Abstractions;

namespace OCR.Application.Features.Auth.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResult>
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenRepository;

        public LoginUserCommandHandler(UserManager<IdentityUser> userManager, ITokenService tokenRepository)
        {
            _userManager = userManager;
            _tokenRepository = tokenRepository;
        }

        public async Task<LoginUserResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(request.Username);

            if (user != null)
            {
                var checkPassword = await _userManager.CheckPasswordAsync(user, request.Password);

                if (checkPassword)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles != null)
                    {
                        var jwtToken = _tokenRepository.CreateJWTToken(user, roles.ToList());

                        return new LoginUserResult(
                            _jwtToken: jwtToken);
                    }
                }
            }

            throw new UnauthorizedAccessException("Invalid username or password");
        }
    }
    }

