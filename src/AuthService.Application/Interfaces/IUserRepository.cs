
using AuthService.Domain.Entities;
namespace AuthService.Application.Interfaces;
public interface IUserRepository
{
    Task<User?> GetByPhoneAsync(string phone);
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
