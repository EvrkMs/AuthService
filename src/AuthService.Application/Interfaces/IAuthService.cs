
using AuthService.Application.DTOs;
namespace AuthService.Application.Interfaces;
public interface IAuthService
{
    Task<(TokenResponse,string)> LoginAsync(LoginRequest req,string deviceId);
    Task RegisterAsync(RegisterRequest req);
    Task<TokenResponse> RefreshAsync(string combined,string deviceId);
    Task LogoutAsync(string combined);
    Task<IEnumerable<SessionDto>> GetActiveSessionsAsync(Guid userId);
    Task RevokeAllSessionsAsync(Guid userId);
    Task ChangePasswordAsync(Guid userId,string oldPassword,string newPassword);
}
