using MediatR;
using Microsoft.AspNetCore.Identity;
using OCR.Application.Abstractions;
using OCR.Application.Common.Exceptions;

namespace OCR.Application.Features.Auth.RegisterUser
{

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
    {

        private readonly UserManager<IdentityUser> userManager;
        private readonly ITokenService tokenRepository;

        public RegisterUserCommandHandler(UserManager<IdentityUser> userManager, ITokenService tokenRepository)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
        }

        public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var identityUser = new IdentityUser
            {
                UserName = request.Username,
                Email = request.Username
            };

            var identityResult = await userManager.CreateAsync(identityUser, request.Password);

            if (identityResult.Succeeded)
            {
                if (request.Roles != null && request.Roles.Length > 0)
                {
                    identityResult = await userManager.AddToRolesAsync(identityUser, new[] { request.Roles });
                    if (identityResult.Succeeded)
                    {
                        return new RegisterUserResult(
                            Username: request.Username
                            );
                    }
                }
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
    
