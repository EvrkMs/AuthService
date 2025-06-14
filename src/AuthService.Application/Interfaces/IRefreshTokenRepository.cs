
using AuthService.Domain.Entities;
namespace AuthService.Application.Interfaces;
public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetAsync(string hash);
    Task<List<RefreshToken>> GetByUserIdAsync(Guid userId);
    Task SaveChangesAsync();
}
