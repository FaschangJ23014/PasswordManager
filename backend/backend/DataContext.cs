using Microsoft.EntityFrameworkCore;
namespace backend;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<PasswordEntry> Passwords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Zwingt die Tabelle auf den Namen "passwords" im public-Schema
        var entity = modelBuilder.Entity<PasswordEntry>();
        entity.ToTable("passwords", schema: "public");

        // Zwingt alle Spaltennamen auf exakte Kleinbuchstaben, damit Postgres sie findet!
        entity.Property(p => p.Id).HasColumnName("id");
        entity.Property(p => p.Website).HasColumnName("website");
        entity.Property(p => p.Username).HasColumnName("username");
        entity.Property(p => p.EncryptedPassword).HasColumnName("encryptedpassword");
    }
}