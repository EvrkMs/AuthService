
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
        npg => npg.MigrationsAssembly("AuthService.Api")));


builder.Services.AddScoped<IUserRepository,UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository,RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordHasher,BcryptPasswordHasher>();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddScoped<IAuthService,AuthService.Application.Services.AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Введите 'Bearer {token}'"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


var app=builder.Build();
using(var scope=app.Services.CreateScope()){
    var db=scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    if(!db.Users.Any(u=>u.Roles.Contains("Owner")))
    {
        var user = new AuthService.Domain.Entities.User
        {
            Phone = "owner",
            PasswordHash = hasher.Hash("owner")
        };
        user.Roles.Add("Owner");
        db.Users.Add(user);
        db.SaveChanges();
    }
}
app.UseExceptionHandler(a=>a.Run(async ctx=>{
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>()?.Error;
    if(ex is InvalidOperationException)
    {
        ctx.Response.StatusCode=400;
        await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    else
    {
        ctx.Response.StatusCode=500;
        await ctx.Response.WriteAsJsonAsync(new { error = "Internal Server Error" });
    }
}));
app.UseSwagger(); app.UseSwaggerUI();
app.UseAuthentication(); app.UseAuthorization();

app.MapControllers();

app.MapJwks();

app.Run();
