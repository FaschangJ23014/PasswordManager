namespace backend;

public class PasswordsService
{
    private readonly DataContext _context;
    public PasswordsService(DataContext context )
    {
        _context = context;
    }

    public List<PasswordEntry> GetAllPasswords()
    {
        return _context.Passwords.ToList();
    }

    public PasswordEntry CreatePassword(PasswordEntry entry)
    {
        _context.Passwords.Add(entry);
        _context.SaveChanges();
        return entry;
    }

    public List<PasswordEntry> SearchPasswords(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return _context.Passwords.ToList();
        }

        return _context.Passwords
            .Where(p => p.Website.ToLower().Contains(query.ToLower()))
            .ToList();
    }

}
