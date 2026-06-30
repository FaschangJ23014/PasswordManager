using System.Security.Cryptography;
using System.Text;

namespace backend;

public class PasswordsService
{
    private readonly DataContext _context;
    private readonly string _encryptionKey;

    public PasswordsService(DataContext context, IConfiguration configuration)
    {
        _context = context;
        // Liest aus: ShieldSettings -> EncryptionKey
        _encryptionKey = configuration["ShieldSettings:EncryptionKey"] ?? "StandardFallbackKey32ZeichenLang!";
    }

    // HIER DIE NEUE HILFSMETHODE: Zwingt den Key IMMER auf exakt 32 Bytes (256 Bit)
    private byte[] GetSecureKeyBytes()
    {
        byte[] keyBytes = new byte[32]; // AES-256 braucht genau 32 Bytes
        byte[] secretBytes = Encoding.UTF8.GetBytes(_encryptionKey);

        // Kopiert die Bytes und füllt den Rest mit 0 auf oder schneidet nach 32 Bytes ab
        Array.Copy(secretBytes, keyBytes, Math.Min(secretBytes.Length, keyBytes.Length));
        return keyBytes;
    }

    public List<PasswordEntry> GetAllPasswords()
    {
        var entries = _context.Passwords.ToList();

        foreach (var entry in entries)
        {
            entry.EncryptedPassword = Decrypt(entry.EncryptedPassword);
        }

        return entries;
    }

    public PasswordEntry CreatePassword(PasswordEntry entry)
    {
        entry.EncryptedPassword = Encrypt(entry.EncryptedPassword);

        _context.Passwords.Add(entry);
        _context.SaveChanges();

        entry.EncryptedPassword = Decrypt(entry.EncryptedPassword);
        return entry;
    }

    public List<PasswordEntry> SearchPasswords(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return GetAllPasswords();
        }

        var filtered = _context.Passwords
            .Where(p => p.Website.ToLower().Contains(query.ToLower()))
            .ToList();

        foreach (var entry in filtered)
        {
            entry.EncryptedPassword = Decrypt(entry.EncryptedPassword);
        }

        return filtered;
    }

    public void Delete(int id)
    {
        var password = _context.Passwords.FirstOrDefault(x =>  x.Id == id);

        if(password == null)
        {
            throw new Exception("Object not found");
        }

        _context.Passwords.Remove(password);
        _context.SaveChanges();

    }

    // --- AES VERSCHLÜSSELUNG LOGIK ---
    private string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        using Aes aes = Aes.Create();
        aes.Key = GetSecureKeyBytes(); // <-- Nutzt jetzt die sichere 32-Byte Methode!
        aes.IV = new byte[16];

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    // --- AES ENTSCHLÜSSELUNG LOGIK ---
    private string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        try
        {
            using Aes aes = Aes.Create();
            aes.Key = GetSecureKeyBytes(); // <-- Nutzt jetzt die sichere 32-Byte Methode!
            aes.IV = new byte[16];

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
        catch
        {
            return "[FEHLER BEIM ENTSCHLÜSSELN]";
        }
    }
}