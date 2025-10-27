using Microsoft.AspNetCore.Identity;

namespace OCR.Repositories
{
    public interface ITokenRepository
    {
        string CreateJWTToken(IdentityUser user, List<string> roles);
    }
}
