
using AuthService.Application.DTOs;
namespace AuthService.Application.Interfaces;
public interface IAuthService
{
    Task<(TokenResponse,string)> LoginAsync(LoginRequest req,string deviceId);
    Task<TokenResponse> RegisterAsync(RegisterRequest req);
    Task<TokenResponse> RefreshAsync(string combined,string deviceId);
    Task LogoutAsync(string combined);
}
