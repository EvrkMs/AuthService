
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class UserRepository(AuthDbContext db): IUserRepository
{
    public Task<User?> GetByEmailAsync(string email)=> db.Users.FirstOrDefaultAsync(u=>u.Email==email);
    public Task AddAsync(User user)=> db.Users.AddAsync(user).AsTask();
    public Task SaveChangesAsync()=> db.SaveChangesAsync();
}
