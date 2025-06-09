
using AuthService.Domain.Entities;
namespace AuthService.Application.Interfaces;
public interface IJwtService
{
    string CreateAccessToken(User user,string deviceId);
}
