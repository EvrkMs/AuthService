
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class UserRepository(AuthDbContext db): IUserRepository
{
    public Task<User?> GetByPhoneAsync(string phone)=> db.Users.FirstOrDefaultAsync(u=>u.Phone==phone);
    public Task<User?> GetByIdAsync(Guid id)=> db.Users.FirstOrDefaultAsync(u=>u.Id==id);
    public Task AddAsync(User user)=> db.Users.AddAsync(user).AsTask();
    public Task SaveChangesAsync()=> db.SaveChangesAsync();
}
