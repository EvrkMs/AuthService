
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService service,IConfiguration cfg): ControllerBase
{
    private readonly bool secure= cfg.GetValue("COOKIE_SECURE", true);
    private readonly int days = int.Parse(cfg["Jwt:RefreshTokenExpireDays"] ?? "7");

    [HttpPost("register")] [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest req){
        await service.RegisterAsync(req);
        return Ok();
    }
    [HttpPost("login")] [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest req){
        var dev = Request.Headers["X-Device-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        var agent = Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var lang = Request.Headers["Accept-Language"].FirstOrDefault() ?? "unknown";
        var secUa = Request.Headers["sec-ch-ua"].FirstOrDefault() ?? "unknown";
        var platform = Request.Headers["sec-ch-ua-platform"].FirstOrDefault() ?? "unknown";
        var mobile = Request.Headers["sec-ch-ua-mobile"].FirstOrDefault() ?? "unknown";
        req.DeviceInfo = JsonSerializer.Serialize(new {
            agent,
            ip,
            lang,
            secUa,
            platform,
            mobile
        });
        
        var (token,refresh)=await service.LoginAsync(req,dev);
        Response.Cookies.Append("refreshToken",refresh,new CookieOptions{
            HttpOnly=true,Secure=secure,SameSite=SameSiteMode.Strict,
            Expires=DateTimeOffset.UtcNow.AddDays(days)
        });
        return Ok(token);
    }
    [HttpPost("refresh")] [AllowAnonymous]
    public async Task<IActionResult> Refresh(){
        if(!Request.Cookies.TryGetValue("refreshToken",out var combined)) return Unauthorized();
        var dev=Request.Headers["X-Device-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        var token=await service.RefreshAsync(combined!,dev);
        return Ok(token);
    }
    [HttpPost("logout")] [Authorize]
    public async Task<IActionResult> Logout(){
        if(Request.Cookies.TryGetValue("refreshToken",out var comb)){
            await service.LogoutAsync(comb!); Response.Cookies.Delete("refreshToken");
        }
        return Ok();
    }
}
