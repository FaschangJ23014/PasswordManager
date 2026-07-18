using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;  
using System.IdentityModel.Tokens.Jwt;      
using Microsoft.Extensions.Configuration;    

namespace backend;

public class AuthService
{
    //Standard-Hasher von ASP.NET Core
    private readonly PasswordHasher<Users> _hasher = new();
    private readonly IConfiguration _config;

    public AuthService(IConfiguration config)
    {
        _config = config;
    }

    //Für Tokenerstellung
    public string CreateToken(Users user)
    {
        // 1. Claims: Das sind die Informationen, die im Token stecken sollen
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        // 2. Secret Key: Der wird aus der Konfiguration gelesen(Render Environment)
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["JWT:SecretKey"] ?? "DiesIstEinStandardKeyDerNurZumTestenDient"));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        // 3. Token zusammenbauen
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(1), // Token ist 1 Tag gültig
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string HashPassword(Users user, string password) 
        => _hasher.HashPassword(user, password);

    public bool VerifyPassword(Users user, string hashedPassword, string providedPassword)
        => _hasher.VerifyHashedPassword(user, hashedPassword, providedPassword) == PasswordVerificationResult.Success;
}
