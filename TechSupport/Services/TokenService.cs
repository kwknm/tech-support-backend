using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TechSupport.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string GenerateToken(string userId, string email, IEnumerable<string> roles, string firstName, string lastName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
        var tokenHandler = new JwtSecurityTokenHandler();
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Name, firstName),
            new Claim(ClaimTypes.Surname, lastName)
        };
        
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(configuration["Jwt:ExpiresInMinutes"])),
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}