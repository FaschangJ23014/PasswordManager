using Microsoft.EntityFrameworkCore;
namespace backend;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    // Das wird deine einzige Tabelle in der SQLite-Datenbank
    public DbSet<PasswordEntry> Passwords { get; set; }
}