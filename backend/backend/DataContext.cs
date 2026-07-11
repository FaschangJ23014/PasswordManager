using Microsoft.EntityFrameworkCore;
namespace backend;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<PasswordEntry> Passwords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PasswordEntry>()
            .ToTable("Passwords");
    }
}