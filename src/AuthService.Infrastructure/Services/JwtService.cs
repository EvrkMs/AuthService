
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using Microsoft.Extensions.Configuration;

namespace AuthService.Infrastructure.Services;
public class JwtService : IJwtService
{
    private readonly JwtSecurityTokenHandler _h = new();
    private readonly RsaSecurityKey _key;
    private readonly string _iss;
    private readonly string _aud;
    private readonly int _exp;
    public JwtService(IConfiguration cfg)
    {
        _iss = cfg["Jwt:Issuer"]!;
        _aud = cfg["Jwt:Audience"]!;
        _exp = int.Parse(cfg["Jwt:TokenExpireMinutes"] ?? "15");
        var priv = File.ReadAllText(cfg["Jwt:RsaPrivateKeyPath"]!);
        var rsa = RSA.Create();
        rsa.ImportFromPem(priv);
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
