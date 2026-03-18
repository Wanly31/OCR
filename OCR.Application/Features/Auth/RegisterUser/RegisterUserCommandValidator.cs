using FluentValidation;

namespace OCR.Application.Features.Auth.RegisterUser
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator() 
        {

            RuleFor(x => x.Username)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Email is not valid");
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6)
                .WithMessage("Password is not valid");
        } 
    
    }
}
