using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordsDb;

public class PasswordEntry
{
    public int Id { get; set; }
    public string Website { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string EncryptedPassword { get; set; } = string.Empty;
}
