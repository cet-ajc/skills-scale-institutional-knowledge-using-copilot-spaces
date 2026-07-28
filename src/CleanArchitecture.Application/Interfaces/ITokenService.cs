using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
    DateTime GetTokenExpiration();
}
