using Microsoft.AspNetCore.Identity;

namespace backend;

public class UserSettingsService
{
    private readonly DataContext _context;
    private readonly PasswordHasher<Users> _hasher = new();

    public UserSettingsService(DataContext context)
    {
        _context = context;
    }

    public bool UpdateUsername(int id, string username)
    {
        var user = _context.Users.FirstOrDefault(x => x.Id == id);
        if(user == null)
        {
            return false;
        }

        user.Username = username;
        _context.SaveChanges();
        return true;
    }

    public bool UpdatePassword(int id, string newPassword, string oldPassword)
    {
        var user = _context.Users.FirstOrDefault(_ => _.Id == id);
        if(user == null )
        {
            return false;
        }

        var verificationResult = _hasher.VerifyHashedPassword(user, user.PasswordHash, oldPassword);

        if(verificationResult == PasswordVerificationResult.Failed)
        {
            return false;
        }

        user.PasswordHash = _hasher.HashPassword(user, newPassword);
        _context.SaveChanges();
        return true;
    }

}
