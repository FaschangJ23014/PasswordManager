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

    public List<PasswordEntry> GetAllForUser(int userId, string? search)
    {

        var query = _context.Passwords.AsNoTracking().Where(p => p.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Website.ToLower().Contains(search.ToLower()));
        }

        var entries = query.ToList();
        foreach (var entry in entries)
            entry.EncryptedPassword = Decrypt(entry.EncryptedPassword);

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

    public void DeleteForUser(int id, int userId)
    {
        var password = _context.Passwords.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (password == null) throw new Exception("Eintrag nicht gefunden oder keine Berechtigung");

        _context.Passwords.Remove(password);
        _context.SaveChanges();
    }

    // --- AES VERSCHLÜSSELUNG LOGIK ---
    private string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        using Aes aes = Aes.Create();
        aes.Key = GetSecureKeyBytes(); // <-- Nutzt jetzt die sichere 32-Byte Methode!

        aes.GenerateIV();
        byte[] iv = aes.IV;

        using var encryptor = aes.CreateEncryptor(aes.Key, iv);
        using var ms = new MemoryStream();

        ms.Write(iv, 0, iv.Length);

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
            // 1. Den Base64-Salat wieder in rohe Bytes zurückverwandeln
            byte[] fullCipher = Convert.FromBase64String(cipherText);

            using Aes aes = Aes.Create();
            aes.Key = GetSecureKeyBytes();

            // 2. DEN IV WIEDERHERSTELLEN: 
            byte[] iv = new byte[16];
            Array.Copy(fullCipher, 0, iv, 0, 16);
            aes.IV = iv;

            // 3. Der restliche Text nach den ersten 16 Bytes ist der echte verschlüsselte Inhalt
            using var ms = new MemoryStream(fullCipher, 16, fullCipher.Length - 16);
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            // 4. Klartext auslesen und zurückgeben
            return sr.ReadToEnd();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"++++ Krypto-Fehler bei Eintrag: {ex.Message}");
            return "[Entschlüsselung fehlgeschlagen - Falscher Key/Format]";
        }
    }
}