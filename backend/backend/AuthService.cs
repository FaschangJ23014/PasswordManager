using Microsoft.AspNetCore.Identity;

namespace backend;

public class AuthService
{
    private readonly PasswordHasher<Users> _hasher = new();

    public string HashPassword(Users user, string password) 
        => _hasher.HashPassword(user, password);

    public bool VerifyPassword(Users user, string hashedPassword, string providedPassword)
        => _hasher.VerifyHashedPassword(user, hashedPassword, providedPassword) == PasswordVerificationResult.Success;
}
