using Microsoft.AspNetCore.Identity; 

namespace OCR.Domain.Interfaces
{
    public interface ITokenRepository
    {
        string CreateJWTToken(IdentityUser user, List<string> roles);
    }
}
