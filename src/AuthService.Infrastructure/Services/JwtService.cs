
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuthService.Infrastructure.Services;
public class JwtService(IConfiguration cfg): IJwtService
{
    private readonly JwtSecurityTokenHandler _h = new();
    private readonly RsaSecurityKey _key;
    private readonly string _iss = cfg["Jwt:Issuer"]!;
    private readonly string _aud = cfg["Jwt:Audience"]!;
    private readonly int _exp = int.Parse(cfg["Jwt:TokenExpireMinutes"] ?? "15");
    public JwtService(IConfiguration cfg)
    {
        var priv = File.ReadAllText(cfg["Jwt:RsaPrivateKeyPath"]!);
        var rsa = RSA.Create(); rsa.ImportFromPem(priv);
        _key = new RsaSecurityKey(rsa);
    }
    public string CreateAccessToken(User user,string deviceId)
    {
        var claims = new List<Claim>{ new("userId",user.Id.ToString()), new("deviceId",deviceId)};
        claims.AddRange(user.Roles.Select(r=>new Claim("role",r)));
        var token=new JwtSecurityToken(_iss,_aud,claims,expires:DateTime.UtcNow.AddMinutes(_exp),
            signingCredentials:new SigningCredentials(_key,SecurityAlgorithms.RsaSha256));
        return _h.WriteToken(token);
    }
}
