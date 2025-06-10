using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/auth")] 
[Authorize]
public class SessionsController(IAuthService service) : ControllerBase
{
    [HttpGet("sessions")]
    public async Task<IEnumerable<SessionDto>> Get()
    {
        var id = Guid.Parse(User.FindFirst("userId")!.Value);
        return await service.GetActiveSessionsAsync(id);
    }

    [HttpPost("sessions/revoke-all")]
    public async Task<IActionResult> RevokeAll()
    {
        var id = Guid.Parse(User.FindFirst("userId")!.Value);
        await service.RevokeAllSessionsAsync(id);
        Response.Cookies.Delete("refreshToken");
        return Ok();
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest req)
    {
        var id = Guid.Parse(User.FindFirst("userId")!.Value);
        await service.ChangePasswordAsync(id, req.OldPassword, req.NewPassword);
        Response.Cookies.Delete("refreshToken");
        return Ok();
    }
}
