using AuthService.Application.Interfaces;
using AuthService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AuthService.Infrastructure;

public static class JwtSetupExtensions
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration cfg)
    {
        var rsa = RSA.Create(2048);
        var key = new RsaSecurityKey(rsa);
        services.AddSingleton(rsa);
        services.AddSingleton(key);
        services.AddSingleton<IJwtService, JwtService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = cfg["Jwt:Issuer"],
                    ValidAudience = cfg["Jwt:Audience"],
                    IssuerSigningKey = key
                };
            });
        return services;
    }

    public static WebApplication MapJwks(this WebApplication app)
    {
        var rsa = app.Services.GetRequiredService<RSA>();
        app.MapGet("/.well-known/jwks.json", () =>
        {
            var p = rsa.ExportParameters(false);
            var jwk = new JsonWebKey
            {
                Kty = "RSA",
                Alg = SecurityAlgorithms.RsaSha256,
                Use = "sig",
                N = Base64UrlEncoder.Encode(p.Modulus),
                E = Base64UrlEncoder.Encode(p.Exponent)
            };
            return Results.Json(new { keys = new[] { jwk } });
        });
        return app;
    }
}
