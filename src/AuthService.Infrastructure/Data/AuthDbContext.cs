using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;      // 👈
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using AuthService.Domain.Entities;

namespace AuthService.Infrastructure.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> opts) : DbContext(opts)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---------- Конвертер + компаратор для List<string>
        var rolesConverter = new ValueConverter<List<string>, string>(
            v => string.Join(',', v),                                   // → в БД
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)    // ← из БД
                  .ToList());

        var rolesComparer = new ValueComparer<List<string>>(
            (l1, l2) => l1.SequenceEqual(l2),                           // Equality
            l => l.Aggregate(0, (c, v) => HashCode.Combine(c, v.GetHashCode())), // Hash
            l => l.ToList());                                          // Deep-copy

        // ---------- User
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.HasIndex(u => u.Email).IsUnique();

            b.Property(u => u.Roles)
             .HasConversion(rolesConverter)
             .Metadata.SetValueComparer(rolesComparer);                // 👈 важная строка
        });

        // ---------- RefreshToken
        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.HasKey(t => t.Id);
            b.HasIndex(t => t.TokenHash).IsUnique();
            b.HasOne(t => t.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(t => t.UserId);
        });
    }
}
