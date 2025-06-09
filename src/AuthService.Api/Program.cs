
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<AuthDbContext>(opt=>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IUserRepository,UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository,RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordHasher,BcryptPasswordHasher>();
builder.Services.AddSingleton<IJwtService,JwtService>();
builder.Services.AddScoped<IAuthService,AuthService.Application.Services.AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt=>{
        var cfg=builder.Configuration;
        opt.TokenValidationParameters=new TokenValidationParameters{
            ValidateIssuer=true,ValidateAudience=true,ValidateLifetime=true,ValidateIssuerSigningKey=true,
            ValidIssuer=cfg["Jwt:Issuer"],ValidAudience=cfg["Jwt:Audience"],
            IssuerSigningKeyResolver=(token,sec,kid,param)=>{
                var rsa=RSA.Create(); rsa.ImportFromPem(File.ReadAllText(cfg["Jwt:RsaPublicKeyPath"]!));
                return new[]{new RsaSecurityKey(rsa)};
            }
        };
    });

var app=builder.Build();
using(var scope=app.Services.CreateScope()){
    var db=scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}
app.UseSwagger(); app.UseSwaggerUI();
app.UseAuthentication(); app.UseAuthorization();

app.MapControllers();

app.MapGet("/.well-known/jwks.json",(IConfiguration cfg)=>{
    var rsa=RSA.Create(); rsa.ImportFromPem(File.ReadAllText(cfg["Jwt:RsaPublicKeyPath"]!));
    var p=rsa.ExportParameters(false);
    var jwk=new Microsoft.IdentityModel.Tokens.JsonWebKey{
        Kty="RSA",Alg=SecurityAlgorithms.RsaSha256,Use="sig",
        N=Base64UrlEncoder.Encode(p.Modulus),E=Base64UrlEncoder.Encode(p.Exponent)
    };
    return Results.Json(new{keys=new[]{jwk}});
});

app.Run();
