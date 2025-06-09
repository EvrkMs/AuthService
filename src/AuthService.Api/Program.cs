
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using AuthService.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<AuthDbContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        npg => npg.MigrationsAssembly("AuthService.Infrastructure")));


builder.Services.AddScoped<IUserRepository,UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository,RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordHasher,BcryptPasswordHasher>();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddScoped<IAuthService,AuthService.Application.Services.AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app=builder.Build();
using(var scope=app.Services.CreateScope()){
    var db=scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}
app.UseSwagger(); app.UseSwaggerUI();
app.UseAuthentication(); app.UseAuthorization();

app.MapControllers();

app.MapJwks();

app.Run();
