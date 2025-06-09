
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class RefreshTokenRepository(AuthDbContext db): IRefreshTokenRepository
{
    public Task AddAsync(RefreshToken t)=> db.RefreshTokens.AddAsync(t).AsTask();
    public Task<RefreshToken?> GetAsync(string hash)=> db.RefreshTokens.Include(t=>t.User).FirstOrDefaultAsync(t=>t.TokenHash==hash);
    public Task SaveChangesAsync()=> db.SaveChangesAsync();
}
