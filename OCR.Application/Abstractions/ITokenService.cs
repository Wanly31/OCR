using Microsoft.AspNetCore.Identity;

namespace OCR.Application.Abstractions
{
    public interface ITokenService
    {
        string CreateJWTToken(IdentityUser user, List<string> roles);
    }
}
