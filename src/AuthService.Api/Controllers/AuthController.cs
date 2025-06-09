
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService service,IConfiguration cfg): ControllerBase
{
    private readonly bool secure= bool.Parse(cfg["COOKIE_SECURE"] ?? "false");
    private readonly int days = int.Parse(cfg["Jwt:RefreshTokenExpireDays"] ?? "7");

    [HttpPost("register")] [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest req){
        var dev=Guid.NewGuid().ToString();
        var token=await service.RegisterAsync(req,dev);
        return Ok(token);
    }
    [HttpPost("login")] [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest req){
        var dev=Guid.NewGuid().ToString();
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
        var dev=Guid.NewGuid().ToString();
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
