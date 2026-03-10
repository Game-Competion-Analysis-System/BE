using DAL.Entities;

namespace BIL.Service;

public interface IAuthService
{
    string GenerateJwtToken(User user);
    User? Authenticate(string email, string password);
}
