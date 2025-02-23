namespace TechSupport.Services;

public interface ITokenService
{
    string GenerateToken(string userId, string email, IEnumerable<string> roles, string firstName, string lastName);
}