using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PasswordsDb;

public partial class PasswordsContext : DbContext
{
    public PasswordsContext(DbContextOptions<PasswordsContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
