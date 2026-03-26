using MediatR;
using Microsoft.AspNetCore.Identity;
using OCR.Application.Abstractions;
using OCR.Application.Common.Exceptions;
using System.Data;

namespace OCR.Application.Features.Auth.RegisterUser
{

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenRepository;

        public RegisterUserCommandHandler(UserManager<IdentityUser> userManager, ITokenService tokenRepository)
        {
            _userManager = userManager;
            _tokenRepository = tokenRepository;
        }

        public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var identityUser = new IdentityUser
            {
                UserName = request.Username,
                Email = request.Username
            };

            var identityResult = await _userManager.CreateAsync(identityUser, request.Password);

            if (identityResult.Succeeded)
            {
                identityResult = await _userManager.AddToRoleAsync(identityUser, "Reader");

                if (!identityResult.Succeeded)
                {
                    var roleErrors = new Dictionary<string, string[]>
                    {
                        {
                            "Roles", identityResult.Errors.Select(e => e.Description).ToArray() 
                        }
                    };
                    throw new ValidationException(roleErrors);
                }

                var roles = (await _userManager.GetRolesAsync(identityUser)).ToList();
                var jwtToken = _tokenRepository.CreateJWTToken(identityUser, roles);

                return new RegisterUserResult(_jwtToken: jwtToken);
            }
            var errors = new Dictionary<string, string[]>
            {
                { 
                    "Registration", identityResult.Errors.Select(e => e.Description).ToArray() 
                }
            };
            
            throw new ValidationException(errors);
        }
    }
}
    
