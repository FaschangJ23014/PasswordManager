using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PasswordsDb;

[Table("users", Schema = "public")]
public class Users
{
    [Key]
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public List<PasswordEntry> PasswordEntries { get; set; } = new();

}
