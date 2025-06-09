
using System.Security.Cryptography;
using System.Text;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace AuthService.Application.Services;

public class AuthService(IUserRepository users,
                         IRefreshTokenRepository tokens,
                         IPasswordHasher hasher,
                         IJwtService jwt,
                         IConfiguration cfg) : IAuthService
{
    private readonly int accessMinutes = int.Parse(cfg["Jwt:TokenExpireMinutes"] ?? "15");
    private readonly int refreshDays = int.Parse(cfg["Jwt:RefreshTokenExpireDays"] ?? "7");

    public async Task RegisterAsync(RegisterRequest req)
    {
        if(!System.Text.RegularExpressions.Regex.IsMatch(req.Phone, "^(?:\\+7|8)\\d{10}$"))
            throw new InvalidOperationException("Bad phone format");
        if(await users.GetByPhoneAsync(req.Phone) is not null)
            throw new InvalidOperationException("Phone exists");
        var user = new User{Phone=req.Phone,PasswordHash=hasher.Hash(req.Password)};
        user.Roles.Add("Client");
        await users.AddAsync(user);
        await users.SaveChangesAsync();
    }

    public async Task<(TokenResponse,string)> LoginAsync(LoginRequest req,string deviceId)
    {
        var user = await users.GetByPhoneAsync(req.Phone) ?? throw new InvalidOperationException("Bad creds");
        if(!hasher.Verify(req.Password,user.PasswordHash)) throw new InvalidOperationException("Bad creds");
        var (_,plain) = await IssueRefresh(user,deviceId,req.DeviceInfo);
        var access = jwt.CreateAccessToken(user,deviceId);
        return (new TokenResponse(access,accessMinutes*60),plain);
    }

    public async Task<TokenResponse> RefreshAsync(string combined,string deviceId)
    {
        var (salt,plain) = Split(combined);
        var hash = Hash(plain,salt);
        var stored = await tokens.GetAsync(hash) ?? throw new InvalidOperationException("Invalid refresh");
        if(stored.Revoked||stored.ExpiresAt<=DateTime.UtcNow) throw new InvalidOperationException("Expired");
        stored.Revoked=true;
        await tokens.SaveChangesAsync();
        var user = stored.User!;
        await IssueRefresh(user,deviceId,stored.DeviceInfo);
        var access = jwt.CreateAccessToken(user,deviceId);
        return new TokenResponse(access,accessMinutes*60);
    }

    public async Task LogoutAsync(string combined)
    {
        var (salt,plain) = Split(combined);
        var hash=Hash(plain,salt);
        var stored = await tokens.GetAsync(hash);
        if(stored!=null){ stored.Revoked=true; await tokens.SaveChangesAsync();}
    }

    private async Task<(string stored,string plain)> IssueRefresh(User user,string deviceId,string? info)
    {
        var plain = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        var hash = Hash(plain,salt);
        var token = new RefreshToken{
            TokenHash=hash,
            Salt=salt,
            DeviceId=deviceId,
            DeviceInfo=info,
            ExpiresAt=DateTime.UtcNow.AddDays(refreshDays),
            UserId=user.Id
        };
        await tokens.AddAsync(token);
        await tokens.SaveChangesAsync();
        return ($"{salt}:{plain}", $"{salt}:{plain}");
    }

    private static string Hash(string data,string salt)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(data+salt)));

    private static (string salt,string plain) Split(string combined)
    {
        var parts=combined.Split(':',2);
        if(parts.Length!=2) throw new InvalidOperationException("Bad token");
        return (parts[0],parts[1]);
    }
}
