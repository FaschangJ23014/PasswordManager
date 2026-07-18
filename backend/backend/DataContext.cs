using Microsoft.EntityFrameworkCore;
namespace backend;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<PasswordEntry> Passwords { get; set; }
    public DbSet<Users> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Zwingt die Tabelle auf den Namen "passwords" im public-Schema
        var entity = modelBuilder.Entity<PasswordEntry>();
        entity.ToTable("passwords", schema: "public");

        // Zwingt alle Spaltennamen auf exakte Kleinbuchstaben, damit Postgres sie findet!
        // Die Backslashes zwingen EF Core, die Spalten im SQL-Befehl in "Id", "Website" etc. zu setzen!
        entity.Property(p => p.Id).HasColumnName("\"Id\"");
        entity.Property(p => p.Website).HasColumnName("\"Website\"");
        entity.Property(p => p.Username).HasColumnName("\"Username\"");
        entity.Property(p => p.EncryptedPassword).HasColumnName("\"EncryptedPassword\"");
    }
}