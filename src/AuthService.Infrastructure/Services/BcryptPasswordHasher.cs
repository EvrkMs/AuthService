
using AuthService.Application.Interfaces;
namespace AuthService.Infrastructure.Services;
public class BcryptPasswordHasher:IPasswordHasher
{
    public string Hash(string p)=>BCrypt.Net.BCrypt.HashPassword(p);
    public bool Verify(string p,string h)=>BCrypt.Net.BCrypt.Verify(p,h);
}
