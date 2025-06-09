
using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Entities;

namespace AuthService.Infrastructure.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> opts): DbContext(opts)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b=>{
            b.HasKey(u=>u.Id);
            b.HasIndex(u=>u.Email).IsUnique();
            b.Property(u=>u.Roles).HasConversion(
                v=>string.Join(',',v),
                s=>s.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
        });
        modelBuilder.Entity<RefreshToken>(b=>{
            b.HasKey(t=>t.Id);
            b.HasIndex(t=>t.TokenHash).IsUnique();
            b.HasOne(t=>t.User).WithMany(u=>u.RefreshTokens).HasForeignKey(t=>t.UserId);
        });
    }
}
